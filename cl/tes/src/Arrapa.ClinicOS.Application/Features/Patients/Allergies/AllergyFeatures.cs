using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Patients;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Patients.Allergies;

// -------------------- List --------------------
public sealed record ListPatientAllergiesQuery(Guid PatientId) : IRequest<Result<IReadOnlyList<PatientAllergyResponse>>>;

public sealed class ListPatientAllergiesQueryHandler : IRequestHandler<ListPatientAllergiesQuery, Result<IReadOnlyList<PatientAllergyResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListPatientAllergiesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<PatientAllergyResponse>>> Handle(ListPatientAllergiesQuery request, CancellationToken cancellationToken)
    {
        var patientExists = await _db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken).ConfigureAwait(false);
        if (!patientExists)
        {
            return PatientErrors.NotFound;
        }

        var allergies = await _db.PatientAllergies.AsNoTracking()
            .Where(a => a.PatientId == request.PatientId)
            .OrderByDescending(a => a.Severity)
            .ThenBy(a => a.Name)
            .Select(a => new PatientAllergyResponse(
                a.Id, a.PatientId, a.Name, a.Severity.ToString(), a.ReactionDescription, a.IdentifiedOn, a.CreatedOn))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return allergies;
    }
}

// -------------------- Add --------------------
public sealed record AddPatientAllergyCommand(
    Guid PatientId, string Name, string Severity, string? ReactionDescription, DateOnly? IdentifiedOn)
    : IRequest<Result<PatientAllergyResponse>>;

public sealed class AddPatientAllergyCommandValidator : AbstractValidator<AddPatientAllergyCommand>
{
    public AddPatientAllergyCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReactionDescription).MaximumLength(500);
        RuleFor(x => x.Severity).Must(v => Enum.TryParse<AllergySeverity>(v, ignoreCase: true, out _))
            .WithMessage("Invalid severity.");
    }
}

public sealed class AddPatientAllergyCommandHandler : IRequestHandler<AddPatientAllergyCommand, Result<PatientAllergyResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public AddPatientAllergyCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PatientAllergyResponse>> Handle(AddPatientAllergyCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<PatientAllergyResponse>.Failure(tenantIdResult.Error);
        }

        var patientExists = await _db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken).ConfigureAwait(false);
        if (!patientExists)
        {
            return PatientErrors.NotFound;
        }

        var allergy = new PatientAllergy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            PatientId = request.PatientId,
            Name = request.Name.Trim(),
            Severity = Enum.Parse<AllergySeverity>(request.Severity, ignoreCase: true),
            ReactionDescription = request.ReactionDescription?.Trim(),
            IdentifiedOn = request.IdentifiedOn
        };

        await _db.PatientAllergies.AddAsync(allergy, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new PatientAllergyResponse(
            allergy.Id, allergy.PatientId, allergy.Name, allergy.Severity.ToString(),
            allergy.ReactionDescription, allergy.IdentifiedOn, allergy.CreatedOn);
    }
}

// -------------------- Update --------------------
public sealed record UpdatePatientAllergyCommand(
    Guid Id, string Name, string Severity, string? ReactionDescription, DateOnly? IdentifiedOn)
    : IRequest<Result<PatientAllergyResponse>>;

public sealed class UpdatePatientAllergyCommandValidator : AbstractValidator<UpdatePatientAllergyCommand>
{
    public UpdatePatientAllergyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReactionDescription).MaximumLength(500);
        RuleFor(x => x.Severity).Must(v => Enum.TryParse<AllergySeverity>(v, ignoreCase: true, out _));
    }
}

public sealed class UpdatePatientAllergyCommandHandler : IRequestHandler<UpdatePatientAllergyCommand, Result<PatientAllergyResponse>>
{
    private readonly IApplicationDbContext _db;

    public UpdatePatientAllergyCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<PatientAllergyResponse>> Handle(UpdatePatientAllergyCommand request, CancellationToken cancellationToken)
    {
        var allergy = await _db.PatientAllergies.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (allergy is null)
        {
            return PatientErrors.Allergies.NotFound;
        }

        allergy.Name = request.Name.Trim();
        allergy.Severity = Enum.Parse<AllergySeverity>(request.Severity, ignoreCase: true);
        allergy.ReactionDescription = request.ReactionDescription?.Trim();
        allergy.IdentifiedOn = request.IdentifiedOn;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new PatientAllergyResponse(
            allergy.Id, allergy.PatientId, allergy.Name, allergy.Severity.ToString(),
            allergy.ReactionDescription, allergy.IdentifiedOn, allergy.CreatedOn);
    }
}

// -------------------- Delete --------------------
public sealed record DeletePatientAllergyCommand(Guid Id) : IRequest<Result>;

public sealed class DeletePatientAllergyCommandHandler : IRequestHandler<DeletePatientAllergyCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeletePatientAllergyCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeletePatientAllergyCommand request, CancellationToken cancellationToken)
    {
        var allergy = await _db.PatientAllergies.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (allergy is null)
        {
            return PatientErrors.Allergies.NotFound;
        }
        _db.PatientAllergies.Remove(allergy);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
