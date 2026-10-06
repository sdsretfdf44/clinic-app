using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Patients;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Patients.Medications;

// -------------------- List --------------------
public sealed record ListPatientMedicationsQuery(Guid PatientId, bool IncludeInactive) : IRequest<Result<IReadOnlyList<PatientMedicationResponse>>>;

public sealed class ListPatientMedicationsQueryHandler : IRequestHandler<ListPatientMedicationsQuery, Result<IReadOnlyList<PatientMedicationResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListPatientMedicationsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<PatientMedicationResponse>>> Handle(ListPatientMedicationsQuery request, CancellationToken cancellationToken)
    {
        var patientExists = await _db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken).ConfigureAwait(false);
        if (!patientExists)
        {
            return PatientErrors.NotFound;
        }

        var query = _db.PatientMedications.AsNoTracking().Where(m => m.PatientId == request.PatientId);
        if (!request.IncludeInactive)
        {
            query = query.Where(m => m.IsActive);
        }

        var meds = await query
            .OrderByDescending(m => m.IsActive)
            .ThenByDescending(m => m.StartedOn ?? DateOnly.MinValue)
            .Select(m => new PatientMedicationResponse(
                m.Id, m.PatientId, m.Name, m.Dosage, m.Frequency, m.StartedOn, m.Notes, m.IsActive, m.CreatedOn))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return meds;
    }
}

// -------------------- Add --------------------
public sealed record AddPatientMedicationCommand(
    Guid PatientId, string Name, string? Dosage, string? Frequency, DateOnly? StartedOn, string? Notes, bool IsActive)
    : IRequest<Result<PatientMedicationResponse>>;

public sealed class AddPatientMedicationCommandValidator : AbstractValidator<AddPatientMedicationCommand>
{
    public AddPatientMedicationCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dosage).MaximumLength(100);
        RuleFor(x => x.Frequency).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class AddPatientMedicationCommandHandler : IRequestHandler<AddPatientMedicationCommand, Result<PatientMedicationResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public AddPatientMedicationCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PatientMedicationResponse>> Handle(AddPatientMedicationCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<PatientMedicationResponse>.Failure(tenantIdResult.Error);
        }

        var patientExists = await _db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken).ConfigureAwait(false);
        if (!patientExists)
        {
            return PatientErrors.NotFound;
        }

        var med = new PatientMedication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            PatientId = request.PatientId,
            Name = request.Name.Trim(),
            Dosage = request.Dosage?.Trim(),
            Frequency = request.Frequency?.Trim(),
            StartedOn = request.StartedOn,
            Notes = request.Notes?.Trim(),
            IsActive = request.IsActive
        };

        await _db.PatientMedications.AddAsync(med, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new PatientMedicationResponse(
            med.Id, med.PatientId, med.Name, med.Dosage, med.Frequency,
            med.StartedOn, med.Notes, med.IsActive, med.CreatedOn);
    }
}

// -------------------- Update --------------------
public sealed record UpdatePatientMedicationCommand(
    Guid Id, string Name, string? Dosage, string? Frequency, DateOnly? StartedOn, string? Notes, bool IsActive)
    : IRequest<Result<PatientMedicationResponse>>;

public sealed class UpdatePatientMedicationCommandValidator : AbstractValidator<UpdatePatientMedicationCommand>
{
    public UpdatePatientMedicationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dosage).MaximumLength(100);
        RuleFor(x => x.Frequency).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class UpdatePatientMedicationCommandHandler : IRequestHandler<UpdatePatientMedicationCommand, Result<PatientMedicationResponse>>
{
    private readonly IApplicationDbContext _db;

    public UpdatePatientMedicationCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<PatientMedicationResponse>> Handle(UpdatePatientMedicationCommand request, CancellationToken cancellationToken)
    {
        var med = await _db.PatientMedications.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (med is null)
        {
            return PatientErrors.Medications.NotFound;
        }

        med.Name = request.Name.Trim();
        med.Dosage = request.Dosage?.Trim();
        med.Frequency = request.Frequency?.Trim();
        med.StartedOn = request.StartedOn;
        med.Notes = request.Notes?.Trim();
        med.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new PatientMedicationResponse(
            med.Id, med.PatientId, med.Name, med.Dosage, med.Frequency,
            med.StartedOn, med.Notes, med.IsActive, med.CreatedOn);
    }
}

// -------------------- Delete --------------------
public sealed record DeletePatientMedicationCommand(Guid Id) : IRequest<Result>;

public sealed class DeletePatientMedicationCommandHandler : IRequestHandler<DeletePatientMedicationCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeletePatientMedicationCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeletePatientMedicationCommand request, CancellationToken cancellationToken)
    {
        var med = await _db.PatientMedications.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (med is null)
        {
            return PatientErrors.Medications.NotFound;
        }
        _db.PatientMedications.Remove(med);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
