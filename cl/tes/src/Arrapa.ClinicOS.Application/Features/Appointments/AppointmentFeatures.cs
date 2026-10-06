using Arrapa.ClinicOS.Application.Abstractions.Appointments;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Appointments.Common;
using Arrapa.ClinicOS.Contracts.Appointments;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Appointments;

// ==================== List ====================
public sealed record ListAppointmentsQuery(
    int PageNumber, int PageSize,
    DateTimeOffset? From, DateTimeOffset? To,
    Guid? DoctorId, Guid? PatientId, Guid? DepartmentId,
    string? Status, string? Search) : IRequest<Result<PaginatedList<AppointmentSummaryResponse>>>;

public sealed class ListAppointmentsQueryHandler : IRequestHandler<ListAppointmentsQuery, Result<PaginatedList<AppointmentSummaryResponse>>>
{
    private readonly IApplicationDbContext _db;
    public ListAppointmentsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<PaginatedList<AppointmentSummaryResponse>>> Handle(ListAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .Include(a => a.Department)
            .AsQueryable();

        if (request.From.HasValue) query = query.Where(a => a.ScheduledStart >= request.From);
        if (request.To.HasValue) query = query.Where(a => a.ScheduledStart <= request.To);
        if (request.DoctorId.HasValue) query = query.Where(a => a.DoctorId == request.DoctorId);
        if (request.PatientId.HasValue) query = query.Where(a => a.PatientId == request.PatientId);
        if (request.DepartmentId.HasValue) query = query.Where(a => a.DepartmentId == request.DepartmentId);
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AppointmentStatus>(request.Status, ignoreCase: true, out var s))
        {
            query = query.Where(a => a.Status == s);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(a =>
                a.AppointmentNumber.ToLower().Contains(term) ||
                a.Patient!.MRN.ToLower().Contains(term) ||
                a.Patient!.FirstName.ToLower().Contains(term) ||
                a.Patient!.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var rows = await query
            .OrderBy(a => a.ScheduledStart)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedList<AppointmentSummaryResponse>(
            rows.Select(AppointmentMapping.ToSummary).ToList(),
            totalCount, request.PageNumber, request.PageSize);
    }
}

// ==================== Get ====================
public sealed record GetAppointmentQuery(Guid Id) : IRequest<Result<AppointmentDetailResponse>>;

public sealed class GetAppointmentQueryHandler : IRequestHandler<GetAppointmentQuery, Result<AppointmentDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    public GetAppointmentQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<AppointmentDetailResponse>> Handle(GetAppointmentQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .Include(a => a.Department)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);

        return appointment is null ? AppointmentErrors.NotFound : AppointmentMapping.ToDetail(appointment);
    }
}

// ==================== Book ====================
public sealed record BookAppointmentCommand(
    Guid PatientId, Guid DoctorId, Guid? DepartmentId,
    DateTimeOffset ScheduledStart, int DurationMinutes,
    string? Reason, string? Notes) : IRequest<Result<AppointmentDetailResponse>>;

public sealed class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 480);
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Result<AppointmentDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IAppointmentNumberGenerator _numberGenerator;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _mediator;

    public BookAppointmentCommandHandler(
        IApplicationDbContext db,
        ITenantContext tenantContext,
        IAppointmentNumberGenerator numberGenerator,
        IDateTimeProvider clock,
        ISender mediator)
    {
        _db = db;
        _tenantContext = tenantContext;
        _numberGenerator = numberGenerator;
        _clock = clock;
        _mediator = mediator;
    }

    public async Task<Result<AppointmentDetailResponse>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<AppointmentDetailResponse>.Failure(tenantIdResult.Error);
        }
        var tenantId = tenantIdResult.Value;

        var scheduledEnd = request.ScheduledStart.AddMinutes(request.DurationMinutes);
        if (scheduledEnd <= request.ScheduledStart)
        {
            return AppointmentErrors.InvalidTime;
        }
        if (request.ScheduledStart < _clock.UtcNow.AddMinutes(-5))
        {
            return AppointmentErrors.InPast;
        }

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken).ConfigureAwait(false);
        if (patient is null)
        {
            return AppointmentErrors.PatientNotFound;
        }
        if (!patient.IsActive)
        {
            return AppointmentErrors.PatientInactive;
        }

        var doctor = await _db.Doctors.Include(d => d.Department).FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken).ConfigureAwait(false);
        if (doctor is null)
        {
            return AppointmentErrors.DoctorNotFound;
        }
        if (!doctor.IsActive)
        {
            return AppointmentErrors.DoctorInactive;
        }

        Guid? departmentId = request.DepartmentId ?? doctor.DepartmentId;
        if (departmentId.HasValue)
        {
            var deptExists = await _db.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken).ConfigureAwait(false);
            if (!deptExists)
            {
                return AppointmentErrors.DepartmentNotFound;
            }
        }

        // Conflict check — pending patient requests (Requested) are non-binding and don't block.
        var overlap = await _db.Appointments.AnyAsync(a =>
            a.DoctorId == doctor.Id &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.NoShow &&
            a.Status != AppointmentStatus.Requested &&
            a.ScheduledStart < scheduledEnd &&
            a.ScheduledEnd > request.ScheduledStart,
            cancellationToken).ConfigureAwait(false);
        if (overlap)
        {
            return AppointmentErrors.Overlap;
        }

        // Fee snapshot
        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .Where(t => t.Id == tenantId)
            .Select(t => new { t.Currency, t.DefaultConsultationFee })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var effectiveFee = doctor.ConsultationFee ?? doctor.Department?.DefaultConsultationFee ?? tenant?.DefaultConsultationFee;

        var appointmentNumber = await _numberGenerator.GenerateAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AppointmentNumber = appointmentNumber,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            DepartmentId = departmentId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = scheduledEnd,
            Status = AppointmentStatus.Booked,
            Reason = request.Reason?.Trim(),
            Notes = request.Notes?.Trim(),
            ConsultationFee = effectiveFee,
            Currency = tenant?.Currency ?? "USD"
        };

        await _db.Appointments.AddAsync(appointment, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetAppointmentQuery(appointment.Id), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Reschedule ====================
public sealed record RescheduleAppointmentCommand(
    Guid Id, Guid DoctorId, Guid? DepartmentId,
    DateTimeOffset ScheduledStart, int DurationMinutes,
    string? Reason, string? Notes) : IRequest<Result<AppointmentDetailResponse>>;

public sealed class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 480);
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, Result<AppointmentDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _clock;

    public RescheduleAppointmentCommandHandler(IApplicationDbContext db, ISender mediator, IDateTimeProvider clock)
    {
        _db = db;
        _mediator = mediator;
        _clock = clock;
    }

    public async Task<Result<AppointmentDetailResponse>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (appointment is null)
        {
            return AppointmentErrors.NotFound;
        }
        if (appointment.IsTerminal)
        {
            return AppointmentErrors.Terminal;
        }

        var scheduledEnd = request.ScheduledStart.AddMinutes(request.DurationMinutes);
        if (scheduledEnd <= request.ScheduledStart)
        {
            return AppointmentErrors.InvalidTime;
        }
        if (request.ScheduledStart < _clock.UtcNow.AddMinutes(-5))
        {
            return AppointmentErrors.InPast;
        }

        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken).ConfigureAwait(false);
        if (doctor is null)
        {
            return AppointmentErrors.DoctorNotFound;
        }
        if (!doctor.IsActive)
        {
            return AppointmentErrors.DoctorInactive;
        }

        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId, cancellationToken).ConfigureAwait(false);
            if (!deptExists)
            {
                return AppointmentErrors.DepartmentNotFound;
            }
        }

        var overlap = await _db.Appointments.AnyAsync(a =>
            a.Id != appointment.Id &&
            a.DoctorId == request.DoctorId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.NoShow &&
            a.Status != AppointmentStatus.Requested &&
            a.ScheduledStart < scheduledEnd &&
            a.ScheduledEnd > request.ScheduledStart,
            cancellationToken).ConfigureAwait(false);
        if (overlap)
        {
            return AppointmentErrors.Overlap;
        }

        appointment.DoctorId = request.DoctorId;
        appointment.DepartmentId = request.DepartmentId ?? doctor.DepartmentId;
        appointment.ScheduledStart = request.ScheduledStart;
        appointment.ScheduledEnd = scheduledEnd;
        appointment.Reason = request.Reason?.Trim() ?? appointment.Reason;
        appointment.Notes = request.Notes?.Trim() ?? appointment.Notes;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await _mediator.Send(new GetAppointmentQuery(appointment.Id), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Status transitions ====================
public sealed record TransitionAppointmentCommand(Guid Id, AppointmentStatus NextStatus) : IRequest<Result>;

public sealed class TransitionAppointmentCommandHandler : IRequestHandler<TransitionAppointmentCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly Application.Abstractions.MultiTenancy.ITenantContext _tenantContext;

    public TransitionAppointmentCommandHandler(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        Application.Abstractions.MultiTenancy.ITenantContext tenantContext)
    {
        _db = db;
        _clock = clock;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(TransitionAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (appointment is null) return AppointmentErrors.NotFound;
        if (!appointment.CanTransitionTo(request.NextStatus))
        {
            return AppointmentErrors.IllegalTransition;
        }

        // Approving a patient-submitted request materializes a real booking — enforce doctor-slot overlap now.
        if (appointment.Status == AppointmentStatus.Requested && request.NextStatus == AppointmentStatus.Booked)
        {
            var overlap = await _db.Appointments.AnyAsync(a =>
                a.Id != appointment.Id &&
                a.DoctorId == appointment.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                a.Status != AppointmentStatus.Requested &&
                a.ScheduledStart < appointment.ScheduledEnd &&
                a.ScheduledEnd > appointment.ScheduledStart,
                cancellationToken).ConfigureAwait(false);
            if (overlap)
            {
                return AppointmentErrors.Overlap;
            }
        }

        var now = _clock.UtcNow;
        appointment.Status = request.NextStatus;
        switch (request.NextStatus)
        {
            case AppointmentStatus.Booked: /* approving a request — no timestamp column */ break;
            case AppointmentStatus.Confirmed: appointment.ConfirmedAt = now; break;
            case AppointmentStatus.CheckedIn: appointment.CheckedInAt = now; break;
            case AppointmentStatus.InConsultation: appointment.ConsultationStartedAt = now; break;
            case AppointmentStatus.Completed: appointment.ConsultationEndedAt = now; break;
            case AppointmentStatus.NoShow: /* no timestamp column */ break;
        }

        // -------------------- Phase 7 hook --------------------
        // When entering consultation, materialize a Visit if none exists. When completed, lock the visit.
        if (request.NextStatus == AppointmentStatus.InConsultation)
        {
            var existingVisit = await _db.Visits
                .AnyAsync(v => v.AppointmentId == appointment.Id, cancellationToken).ConfigureAwait(false);
            if (!existingVisit && _tenantContext.TenantId is Guid tid)
            {
                var visit = new Domain.Clinical.Visit
                {
                    Id = Guid.NewGuid(),
                    TenantId = tid,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    AppointmentId = appointment.Id,
                    VisitDate = now,
                    IsLocked = false
                };
                await _db.Visits.AddAsync(visit, cancellationToken).ConfigureAwait(false);
            }
        }

        if (request.NextStatus == AppointmentStatus.Completed)
        {
            var visitToLock = await _db.Visits.FirstOrDefaultAsync(v => v.AppointmentId == appointment.Id, cancellationToken).ConfigureAwait(false);
            if (visitToLock is not null)
            {
                visitToLock.IsLocked = true;
            }

            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == appointment.PatientId, cancellationToken).ConfigureAwait(false);
            if (patient is not null)
            {
                patient.LastVisitAt = now;
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// ==================== Cancel ====================
public sealed record CancelAppointmentCommand(Guid Id, string Reason) : IRequest<Result>;

public sealed class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public CancelAppointmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (appointment is null) return AppointmentErrors.NotFound;
        if (!appointment.CanTransitionTo(AppointmentStatus.Cancelled)) return AppointmentErrors.IllegalTransition;

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = _clock.UtcNow;
        appointment.CancelledByUserId = _currentUser.UserId;
        appointment.CancellationReason = request.Reason.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// ==================== Delete (hard remove — soft via interceptor) ====================
public sealed record DeleteAppointmentCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteAppointmentCommandHandler : IRequestHandler<DeleteAppointmentCommand, Result>
{
    private readonly IApplicationDbContext _db;
    public DeleteAppointmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (appointment is null) return AppointmentErrors.NotFound;
        _db.Appointments.Remove(appointment);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// ==================== Available slots ====================
public sealed record AvailableSlotsQuery(Guid DoctorId, DateOnly Date, int SlotMinutes)
    : IRequest<Result<AvailableSlotsResponse>>;

public sealed class AvailableSlotsQueryHandler : IRequestHandler<AvailableSlotsQuery, Result<AvailableSlotsResponse>>
{
    private readonly IApplicationDbContext _db;
    public AvailableSlotsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<AvailableSlotsResponse>> Handle(AvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        if (request.SlotMinutes < 5 || request.SlotMinutes > 240)
        {
            return AppointmentErrors.InvalidDuration;
        }

        var doctor = await _db.Doctors.AnyAsync(d => d.Id == request.DoctorId, cancellationToken).ConfigureAwait(false);
        if (!doctor) return AppointmentErrors.DoctorNotFound;

        var dayOfWeek = (int)request.Date.DayOfWeek;

        // Prefer doctor-specific hours; if none, fall back to clinic-wide.
        var doctorHours = await _db.WorkingHours.AsNoTracking()
            .Where(w => w.DoctorId == request.DoctorId && (int)w.DayOfWeek == dayOfWeek)
            .Select(w => new { w.StartTime, w.EndTime })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (doctorHours.Count == 0)
        {
            doctorHours = await _db.WorkingHours.AsNoTracking()
                .Where(w => w.DoctorId == null && (int)w.DayOfWeek == dayOfWeek)
                .Select(w => new { w.StartTime, w.EndTime })
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        if (doctorHours.Count == 0)
        {
            return new AvailableSlotsResponse(request.DoctorId, request.Date, request.SlotMinutes, Array.Empty<AppointmentSlot>());
        }

        var dayStart = new DateTimeOffset(request.Date.Year, request.Date.Month, request.Date.Day, 0, 0, 0, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);

        var booked = await _db.Appointments.AsNoTracking()
            .Where(a => a.DoctorId == request.DoctorId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && a.Status != AppointmentStatus.Requested
                && a.ScheduledStart < dayEnd
                && a.ScheduledEnd > dayStart)
            .Select(a => new { a.ScheduledStart, a.ScheduledEnd })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var slots = new List<AppointmentSlot>();
        foreach (var window in doctorHours)
        {
            var slotStart = dayStart.Add(window.StartTime.ToTimeSpan());
            var slotEnd = dayStart.Add(window.EndTime.ToTimeSpan());

            var cursor = slotStart;
            while (cursor.AddMinutes(request.SlotMinutes) <= slotEnd)
            {
                var candidateEnd = cursor.AddMinutes(request.SlotMinutes);
                var overlaps = booked.Any(b => b.ScheduledStart < candidateEnd && b.ScheduledEnd > cursor);
                if (!overlaps)
                {
                    slots.Add(new AppointmentSlot(cursor, candidateEnd));
                }
                cursor = candidateEnd;
            }
        }

        return new AvailableSlotsResponse(request.DoctorId, request.Date, request.SlotMinutes, slots);
    }
}
