using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Clinic.WorkingHours;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkingHoursEntity = Arrapa.ClinicOS.Domain.Clinical.WorkingHours;

namespace Arrapa.ClinicOS.Application.Features.Clinic.WorkingHours;

// -------------------- Get --------------------
public sealed record GetWorkingHoursQuery(Guid? DoctorId) : IRequest<Result<WorkingHoursResponse>>;

public sealed class GetWorkingHoursQueryHandler : IRequestHandler<GetWorkingHoursQuery, Result<WorkingHoursResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetWorkingHoursQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<WorkingHoursResponse>> Handle(GetWorkingHoursQuery request, CancellationToken cancellationToken)
    {
        if (request.DoctorId.HasValue)
        {
            var doctorExists = await _db.Doctors.AnyAsync(d => d.Id == request.DoctorId, cancellationToken).ConfigureAwait(false);
            if (!doctorExists)
            {
                return ClinicErrors.WorkingHours.DoctorNotFound;
            }
        }

        var slots = await _db.WorkingHours.AsNoTracking()
            .Where(w => w.DoctorId == request.DoctorId)
            .OrderBy(w => w.DayOfWeek).ThenBy(w => w.StartTime)
            .Select(w => new WorkingHoursSlot(
                (int)w.DayOfWeek,
                w.StartTime.ToString("HH:mm"),
                w.EndTime.ToString("HH:mm")))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new WorkingHoursResponse(request.DoctorId, slots);
    }
}

// -------------------- Set --------------------
public sealed record SetWorkingHoursCommand(Guid? DoctorId, IReadOnlyList<WorkingHoursSlot> Slots)
    : IRequest<Result<WorkingHoursResponse>>;

public sealed class SetWorkingHoursCommandValidator : AbstractValidator<SetWorkingHoursCommand>
{
    public SetWorkingHoursCommandValidator()
    {
        RuleForEach(x => x.Slots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.DayOfWeek).InclusiveBetween(0, 6);
            slot.RuleFor(s => s.StartTime).NotEmpty().Matches(@"^\d{2}:\d{2}$").WithMessage("Time must be HH:mm.");
            slot.RuleFor(s => s.EndTime).NotEmpty().Matches(@"^\d{2}:\d{2}$").WithMessage("Time must be HH:mm.");
        });
    }
}

public sealed class SetWorkingHoursCommandHandler : IRequestHandler<SetWorkingHoursCommand, Result<WorkingHoursResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ISender _mediator;

    public SetWorkingHoursCommandHandler(IApplicationDbContext db, ITenantContext tenantContext, ISender mediator)
    {
        _db = db;
        _tenantContext = tenantContext;
        _mediator = mediator;
    }

    public async Task<Result<WorkingHoursResponse>> Handle(SetWorkingHoursCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<WorkingHoursResponse>.Failure(tenantIdResult.Error);
        }

        if (request.DoctorId.HasValue)
        {
            var doctorExists = await _db.Doctors.AnyAsync(d => d.Id == request.DoctorId, cancellationToken).ConfigureAwait(false);
            if (!doctorExists)
            {
                return ClinicErrors.WorkingHours.DoctorNotFound;
            }
        }

        var parsedSlots = new List<WorkingHoursEntity>();
        foreach (var slot in request.Slots)
        {
            if (!TimeOnly.TryParse(slot.StartTime, out var start) ||
                !TimeOnly.TryParse(slot.EndTime, out var end))
            {
                return Error.Validation("clinic.working_hours.time_format", "Time must be HH:mm.");
            }
            if (start >= end)
            {
                return ClinicErrors.WorkingHours.InvalidSlot;
            }
            parsedSlots.Add(new WorkingHoursEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdResult.Value,
                DoctorId = request.DoctorId,
                DayOfWeek = (DayOfWeek)slot.DayOfWeek,
                StartTime = start,
                EndTime = end
            });
        }

        var existing = await _db.WorkingHours
            .Where(w => w.DoctorId == request.DoctorId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        _db.WorkingHours.RemoveRange(existing);

        if (parsedSlots.Count > 0)
        {
            await _db.WorkingHours.AddRangeAsync(parsedSlots, cancellationToken).ConfigureAwait(false);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetWorkingHoursQuery(request.DoctorId), cancellationToken).ConfigureAwait(false);
    }
}
