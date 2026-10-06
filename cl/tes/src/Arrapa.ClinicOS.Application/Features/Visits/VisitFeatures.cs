using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Visits.Common;
using Arrapa.ClinicOS.Contracts.Visits;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Visits;

// ==================== Get by id ====================
public sealed record GetVisitQuery(Guid Id) : IRequest<Result<VisitDetailResponse>>;

public sealed class GetVisitQueryHandler : IRequestHandler<GetVisitQuery, Result<VisitDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetVisitQueryHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<VisitDetailResponse>> Handle(GetVisitQuery request, CancellationToken cancellationToken)
    {
        var visit = await _db.Visits.AsNoTracking()
            .Include(v => v.Patient)
            .Include(v => v.Doctor).ThenInclude(d => d!.User)
            .Include(v => v.Appointment)
            .Include(v => v.Vitals)
            .Include(v => v.Prescriptions).ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken).ConfigureAwait(false);

        if (visit is null)
        {
            return VisitErrors.NotFound;
        }

        var age = visit.Patient is null ? 0 : ComputeAge(visit.Patient.DateOfBirth, _clock.TodayUtc);
        return VisitMapping.ToDetail(visit, age);
    }

    private static int ComputeAge(DateOnly dob, DateOnly today)
    {
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age))
        {
            age--;
        }
        return age < 0 ? 0 : age;
    }
}

// ==================== Get by appointment ====================
public sealed record GetVisitByAppointmentQuery(Guid AppointmentId) : IRequest<Result<VisitDetailResponse>>;

public sealed class GetVisitByAppointmentQueryHandler : IRequestHandler<GetVisitByAppointmentQuery, Result<VisitDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _mediator;

    public GetVisitByAppointmentQueryHandler(IApplicationDbContext db, ISender mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    public async Task<Result<VisitDetailResponse>> Handle(GetVisitByAppointmentQuery request, CancellationToken cancellationToken)
    {
        var visitId = await _db.Visits.AsNoTracking()
            .Where(v => v.AppointmentId == request.AppointmentId)
            .Select(v => (Guid?)v.Id)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (visitId is null)
        {
            return VisitErrors.NotFound;
        }

        return await _mediator.Send(new GetVisitQuery(visitId.Value), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Upsert vitals ====================
public sealed record UpsertVitalsCommand(
    Guid VisitId,
    int? BloodPressureSystolic, int? BloodPressureDiastolic,
    int? PulseBpm, decimal? TemperatureCelsius,
    decimal? HeightCm, decimal? WeightKg,
    int? SpO2Percent, int? RespirationRate,
    string? Notes) : IRequest<Result<VitalsResponse>>;

public sealed class UpsertVitalsCommandValidator : AbstractValidator<UpsertVitalsCommand>
{
    public UpsertVitalsCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.BloodPressureSystolic).InclusiveBetween(30, 300).When(x => x.BloodPressureSystolic.HasValue);
        RuleFor(x => x.BloodPressureDiastolic).InclusiveBetween(20, 200).When(x => x.BloodPressureDiastolic.HasValue);
        RuleFor(x => x.PulseBpm).InclusiveBetween(20, 250).When(x => x.PulseBpm.HasValue);
        RuleFor(x => x.TemperatureCelsius).InclusiveBetween(25m, 45m).When(x => x.TemperatureCelsius.HasValue);
        RuleFor(x => x.HeightCm).InclusiveBetween(20m, 250m).When(x => x.HeightCm.HasValue);
        RuleFor(x => x.WeightKg).InclusiveBetween(1m, 500m).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.SpO2Percent).InclusiveBetween(50, 100).When(x => x.SpO2Percent.HasValue);
        RuleFor(x => x.RespirationRate).InclusiveBetween(5, 60).When(x => x.RespirationRate.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class UpsertVitalsCommandHandler : IRequestHandler<UpsertVitalsCommand, Result<VitalsResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public UpsertVitalsCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<VitalsResponse>> Handle(UpsertVitalsCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<VitalsResponse>.Failure(tenantIdResult.Error);
        }

        var visit = await _db.Visits.Include(v => v.Vitals).FirstOrDefaultAsync(v => v.Id == request.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null)
        {
            return VisitErrors.NotFound;
        }
        if (visit.IsLocked)
        {
            return VisitErrors.Locked;
        }

        var bmi = VisitMapping.ComputeBmi(request.HeightCm, request.WeightKg);

        if (visit.Vitals is null)
        {
            visit.Vitals = new VisitVitals
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdResult.Value,
                VisitId = visit.Id,
                BloodPressureSystolic = request.BloodPressureSystolic,
                BloodPressureDiastolic = request.BloodPressureDiastolic,
                PulseBpm = request.PulseBpm,
                TemperatureCelsius = request.TemperatureCelsius,
                HeightCm = request.HeightCm,
                WeightKg = request.WeightKg,
                Bmi = bmi,
                SpO2Percent = request.SpO2Percent,
                RespirationRate = request.RespirationRate,
                Notes = request.Notes?.Trim()
            };
        }
        else
        {
            var v = visit.Vitals;
            v.BloodPressureSystolic = request.BloodPressureSystolic;
            v.BloodPressureDiastolic = request.BloodPressureDiastolic;
            v.PulseBpm = request.PulseBpm;
            v.TemperatureCelsius = request.TemperatureCelsius;
            v.HeightCm = request.HeightCm;
            v.WeightKg = request.WeightKg;
            v.Bmi = bmi;
            v.SpO2Percent = request.SpO2Percent;
            v.RespirationRate = request.RespirationRate;
            v.Notes = request.Notes?.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return VisitMapping.ToVitals(visit.Vitals);
    }
}

// ==================== Upsert diagnosis ====================
public sealed record UpsertDiagnosisCommand(
    Guid VisitId,
    string? Symptoms,
    string? Diagnosis,
    string? DoctorNotes) : IRequest<Result<VisitDetailResponse>>;

public sealed class UpsertDiagnosisCommandValidator : AbstractValidator<UpsertDiagnosisCommand>
{
    public UpsertDiagnosisCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Symptoms).MaximumLength(2000);
        RuleFor(x => x.Diagnosis).MaximumLength(2000);
        RuleFor(x => x.DoctorNotes).MaximumLength(4000);
    }
}

public sealed class UpsertDiagnosisCommandHandler : IRequestHandler<UpsertDiagnosisCommand, Result<VisitDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ISender _mediator;

    public UpsertDiagnosisCommandHandler(IApplicationDbContext db, ISender mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    public async Task<Result<VisitDetailResponse>> Handle(UpsertDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == request.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null)
        {
            return VisitErrors.NotFound;
        }
        if (visit.IsLocked)
        {
            return VisitErrors.Locked;
        }

        visit.Symptoms = request.Symptoms?.Trim();
        visit.Diagnosis = request.Diagnosis?.Trim();
        visit.DoctorNotes = request.DoctorNotes?.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await _mediator.Send(new GetVisitQuery(visit.Id), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Add prescription ====================
public sealed record AddPrescriptionCommand(
    Guid VisitId,
    string? GeneralInstructions,
    IReadOnlyList<PrescriptionItemInput> Items) : IRequest<Result<PrescriptionResponse>>;

public sealed record PrescriptionItemInput(
    string Medicine,
    string? Strength,
    string? Dosage,
    string? Frequency,
    string? Duration,
    string? Instructions,
    int SortOrder);

public sealed class AddPrescriptionCommandValidator : AbstractValidator<AddPrescriptionCommand>
{
    public AddPrescriptionCommandValidator()
    {
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.GeneralInstructions).MaximumLength(2000);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one medication is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Medicine).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Strength).MaximumLength(100);
            item.RuleFor(i => i.Dosage).MaximumLength(100);
            item.RuleFor(i => i.Frequency).MaximumLength(100);
            item.RuleFor(i => i.Duration).MaximumLength(100);
            item.RuleFor(i => i.Instructions).MaximumLength(500);
        });
    }
}

public sealed class AddPrescriptionCommandHandler : IRequestHandler<AddPrescriptionCommand, Result<PrescriptionResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;

    public AddPrescriptionCommandHandler(IApplicationDbContext db, ITenantContext tenantContext, IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<PrescriptionResponse>> Handle(AddPrescriptionCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<PrescriptionResponse>.Failure(tenantIdResult.Error);
        }

        var visit = await _db.Visits.FirstOrDefaultAsync(v => v.Id == request.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null)
        {
            return VisitErrors.NotFound;
        }
        if (visit.IsLocked)
        {
            return VisitErrors.Locked;
        }
        if (request.Items.Count == 0)
        {
            return VisitErrors.PrescriptionEmpty;
        }

        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            VisitId = visit.Id,
            IssuedOn = _clock.UtcNow,
            GeneralInstructions = request.GeneralInstructions?.Trim(),
            Items = request.Items.Select(i => new PrescriptionItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdResult.Value,
                Medicine = i.Medicine.Trim(),
                Strength = i.Strength?.Trim(),
                Dosage = i.Dosage?.Trim(),
                Frequency = i.Frequency?.Trim(),
                Duration = i.Duration?.Trim(),
                Instructions = i.Instructions?.Trim(),
                SortOrder = i.SortOrder
            }).ToList()
        };

        await _db.Prescriptions.AddAsync(prescription, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return VisitMapping.ToPrescription(prescription);
    }
}

// ==================== Update prescription ====================
public sealed record UpdatePrescriptionCommand(
    Guid PrescriptionId,
    string? GeneralInstructions,
    IReadOnlyList<PrescriptionItemInput> Items) : IRequest<Result<PrescriptionResponse>>;

public sealed class UpdatePrescriptionCommandValidator : AbstractValidator<UpdatePrescriptionCommand>
{
    public UpdatePrescriptionCommandValidator()
    {
        RuleFor(x => x.PrescriptionId).NotEmpty();
        RuleFor(x => x.GeneralInstructions).MaximumLength(2000);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Medicine).NotEmpty().MaximumLength(200);
        });
    }
}

public sealed class UpdatePrescriptionCommandHandler : IRequestHandler<UpdatePrescriptionCommand, Result<PrescriptionResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public UpdatePrescriptionCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PrescriptionResponse>> Handle(UpdatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<PrescriptionResponse>.Failure(tenantIdResult.Error);
        }

        var prescription = await _db.Prescriptions
            .Include(p => p.Items)
            .Include(p => p.Visit)
            .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId, cancellationToken).ConfigureAwait(false);

        if (prescription is null)
        {
            return VisitErrors.PrescriptionNotFound;
        }
        if (prescription.Visit?.IsLocked ?? false)
        {
            return VisitErrors.Locked;
        }
        if (request.Items.Count == 0)
        {
            return VisitErrors.PrescriptionEmpty;
        }

        prescription.GeneralInstructions = request.GeneralInstructions?.Trim();
        _db.PrescriptionItems.RemoveRange(prescription.Items);

        prescription.Items = request.Items.Select(i => new PrescriptionItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            PrescriptionId = prescription.Id,
            Medicine = i.Medicine.Trim(),
            Strength = i.Strength?.Trim(),
            Dosage = i.Dosage?.Trim(),
            Frequency = i.Frequency?.Trim(),
            Duration = i.Duration?.Trim(),
            Instructions = i.Instructions?.Trim(),
            SortOrder = i.SortOrder
        }).ToList();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return VisitMapping.ToPrescription(prescription);
    }
}

// ==================== Delete prescription ====================
public sealed record DeletePrescriptionCommand(Guid PrescriptionId) : IRequest<Result>;

public sealed class DeletePrescriptionCommandHandler : IRequestHandler<DeletePrescriptionCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeletePrescriptionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeletePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Visit)
            .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId, cancellationToken).ConfigureAwait(false);

        if (prescription is null)
        {
            return VisitErrors.PrescriptionNotFound;
        }
        if (prescription.Visit?.IsLocked ?? false)
        {
            return VisitErrors.Locked;
        }

        _db.Prescriptions.Remove(prescription);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
