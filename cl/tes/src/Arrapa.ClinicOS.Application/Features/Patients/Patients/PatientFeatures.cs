using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Patients;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Patients.Common;
using Arrapa.ClinicOS.Contracts.Patients;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Patients.Patients;

// -------------------- List --------------------
public sealed record ListPatientsQuery(
    int PageNumber, int PageSize, string? Search, string? Gender, bool? IsActive)
    : IRequest<Result<PaginatedList<PatientSummaryResponse>>>;

public sealed class ListPatientsQueryHandler : IRequestHandler<ListPatientsQuery, Result<PaginatedList<PatientSummaryResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public ListPatientsQueryHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<PaginatedList<PatientSummaryResponse>>> Handle(ListPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Patients.AsNoTracking().AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Gender) && Enum.TryParse<Gender>(request.Gender, ignoreCase: true, out var g))
        {
            query = query.Where(p => p.Gender == g);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.MRN.ToLower().Contains(term) ||
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                (p.Phone != null && p.Phone.ToLower().Contains(term)) ||
                (p.Email != null && p.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var rows = await query
            .OrderByDescending(p => p.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var today = _clock.TodayUtc;
        var items = rows.Select(p => PatientMapping.ToSummary(p, today)).ToList();

        return new PaginatedList<PatientSummaryResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

// -------------------- Get --------------------
public sealed record GetPatientQuery(Guid Id) : IRequest<Result<PatientDetailResponse>>;

public sealed class GetPatientQueryHandler : IRequestHandler<GetPatientQuery, Result<PatientDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetPatientQueryHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<PatientDetailResponse>> Handle(GetPatientQuery request, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (patient is null)
        {
            return PatientErrors.NotFound;
        }

        var allergyCount = await _db.PatientAllergies.CountAsync(a => a.PatientId == patient.Id, cancellationToken).ConfigureAwait(false);
        var historyCount = await _db.MedicalHistoryEntries.CountAsync(m => m.PatientId == patient.Id, cancellationToken).ConfigureAwait(false);
        var activeMedicationCount = await _db.PatientMedications.CountAsync(m => m.PatientId == patient.Id && m.IsActive, cancellationToken).ConfigureAwait(false);

        return PatientMapping.ToDetail(patient, _clock.TodayUtc, allergyCount, historyCount, activeMedicationCount);
    }
}

// -------------------- Create --------------------
public sealed record CreatePatientCommand(
    string FirstName, string? MiddleName, string LastName,
    DateOnly DateOfBirth, string Gender, string BloodGroup, string MaritalStatus,
    string? Phone, string? Email, string? PhotoUrl,
    string? AddressLine1, string? AddressLine2, string? City, string? State, string? Country, string? PostalCode,
    string? Occupation,
    string? EmergencyContactName, string? EmergencyContactPhone, string? EmergencyContactRelationship,
    string? GuardianName, string? GuardianPhone, string? GuardianRelationship,
    string? InsuranceProvider, string? InsurancePolicyNumber, string? InsuranceGroupNumber, DateOnly? InsuranceValidUntil,
    string? Notes) : IRequest<Result<PatientDetailResponse>>;

public sealed class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(200);
        RuleFor(x => x.PhotoUrl).MaximumLength(500);
        RuleFor(x => x.Occupation).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.Gender).Must(v => Enum.TryParse<Gender>(v, ignoreCase: true, out _)).WithMessage("Invalid gender.");
        RuleFor(x => x.BloodGroup).Must(v => Enum.TryParse<BloodGroup>(v, ignoreCase: true, out _)).WithMessage("Invalid blood group.");
        RuleFor(x => x.MaritalStatus).Must(v => Enum.TryParse<MaritalStatus>(v, ignoreCase: true, out _)).WithMessage("Invalid marital status.");
    }
}

public sealed class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<PatientDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IPatientMrnGenerator _mrnGenerator;
    private readonly IDateTimeProvider _clock;

    public CreatePatientCommandHandler(
        IApplicationDbContext db,
        ITenantContext tenantContext,
        IPatientMrnGenerator mrnGenerator,
        IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _mrnGenerator = mrnGenerator;
        _clock = clock;
    }

    public async Task<Result<PatientDetailResponse>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<PatientDetailResponse>.Failure(tenantIdResult.Error);
        }

        var today = _clock.TodayUtc;
        if (request.DateOfBirth > today)
        {
            return PatientErrors.InvalidDateOfBirth;
        }

        var mrn = await _mrnGenerator.GenerateAsync(tenantIdResult.Value, cancellationToken).ConfigureAwait(false);

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            MRN = mrn,
            FirstName = request.FirstName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(request.MiddleName) ? null : request.MiddleName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = Enum.Parse<Gender>(request.Gender, ignoreCase: true),
            BloodGroup = Enum.Parse<BloodGroup>(request.BloodGroup, ignoreCase: true),
            MaritalStatus = Enum.Parse<MaritalStatus>(request.MaritalStatus, ignoreCase: true),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim(),
            AddressLine1 = request.AddressLine1?.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            Country = request.Country?.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            Occupation = request.Occupation?.Trim(),
            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactPhone = request.EmergencyContactPhone?.Trim(),
            EmergencyContactRelationship = request.EmergencyContactRelationship?.Trim(),
            GuardianName = request.GuardianName?.Trim(),
            GuardianPhone = request.GuardianPhone?.Trim(),
            GuardianRelationship = request.GuardianRelationship?.Trim(),
            InsuranceProvider = request.InsuranceProvider?.Trim(),
            InsurancePolicyNumber = request.InsurancePolicyNumber?.Trim(),
            InsuranceGroupNumber = request.InsuranceGroupNumber?.Trim(),
            InsuranceValidUntil = request.InsuranceValidUntil,
            Notes = request.Notes?.Trim(),
            IsActive = true
        };

        await _db.Patients.AddAsync(patient, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return PatientMapping.ToDetail(patient, today, 0, 0, 0);
    }
}

// -------------------- Update --------------------
public sealed record UpdatePatientCommand(
    Guid Id,
    string FirstName, string? MiddleName, string LastName,
    DateOnly DateOfBirth, string Gender, string BloodGroup, string MaritalStatus,
    string? Phone, string? Email, string? PhotoUrl,
    string? AddressLine1, string? AddressLine2, string? City, string? State, string? Country, string? PostalCode,
    string? Occupation,
    string? EmergencyContactName, string? EmergencyContactPhone, string? EmergencyContactRelationship,
    string? GuardianName, string? GuardianPhone, string? GuardianRelationship,
    string? InsuranceProvider, string? InsurancePolicyNumber, string? InsuranceGroupNumber, DateOnly? InsuranceValidUntil,
    string? Notes) : IRequest<Result<PatientDetailResponse>>;

public sealed class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(200);
        RuleFor(x => x.PhotoUrl).MaximumLength(500);
        RuleFor(x => x.Occupation).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.Gender).Must(v => Enum.TryParse<Gender>(v, ignoreCase: true, out _));
        RuleFor(x => x.BloodGroup).Must(v => Enum.TryParse<BloodGroup>(v, ignoreCase: true, out _));
        RuleFor(x => x.MaritalStatus).Must(v => Enum.TryParse<MaritalStatus>(v, ignoreCase: true, out _));
    }
}

public sealed class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public UpdatePatientCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<PatientDetailResponse>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (patient is null)
        {
            return PatientErrors.NotFound;
        }

        var today = _clock.TodayUtc;
        if (request.DateOfBirth > today)
        {
            return PatientErrors.InvalidDateOfBirth;
        }

        patient.FirstName = request.FirstName.Trim();
        patient.MiddleName = string.IsNullOrWhiteSpace(request.MiddleName) ? null : request.MiddleName.Trim();
        patient.LastName = request.LastName.Trim();
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = Enum.Parse<Gender>(request.Gender, ignoreCase: true);
        patient.BloodGroup = Enum.Parse<BloodGroup>(request.BloodGroup, ignoreCase: true);
        patient.MaritalStatus = Enum.Parse<MaritalStatus>(request.MaritalStatus, ignoreCase: true);
        patient.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        patient.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        patient.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();
        patient.AddressLine1 = request.AddressLine1?.Trim();
        patient.AddressLine2 = request.AddressLine2?.Trim();
        patient.City = request.City?.Trim();
        patient.State = request.State?.Trim();
        patient.Country = request.Country?.Trim();
        patient.PostalCode = request.PostalCode?.Trim();
        patient.Occupation = request.Occupation?.Trim();
        patient.EmergencyContactName = request.EmergencyContactName?.Trim();
        patient.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
        patient.EmergencyContactRelationship = request.EmergencyContactRelationship?.Trim();
        patient.GuardianName = request.GuardianName?.Trim();
        patient.GuardianPhone = request.GuardianPhone?.Trim();
        patient.GuardianRelationship = request.GuardianRelationship?.Trim();
        patient.InsuranceProvider = request.InsuranceProvider?.Trim();
        patient.InsurancePolicyNumber = request.InsurancePolicyNumber?.Trim();
        patient.InsuranceGroupNumber = request.InsuranceGroupNumber?.Trim();
        patient.InsuranceValidUntil = request.InsuranceValidUntil;
        patient.Notes = request.Notes?.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var allergyCount = await _db.PatientAllergies.CountAsync(a => a.PatientId == patient.Id, cancellationToken).ConfigureAwait(false);
        var historyCount = await _db.MedicalHistoryEntries.CountAsync(m => m.PatientId == patient.Id, cancellationToken).ConfigureAwait(false);
        var activeMedicationCount = await _db.PatientMedications.CountAsync(m => m.PatientId == patient.Id && m.IsActive, cancellationToken).ConfigureAwait(false);

        return PatientMapping.ToDetail(patient, today, allergyCount, historyCount, activeMedicationCount);
    }
}

// -------------------- Toggle Active --------------------
public sealed record SetPatientActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;

public sealed class SetPatientActiveCommandHandler : IRequestHandler<SetPatientActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetPatientActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(SetPatientActiveCommand request, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (patient is null)
        {
            return PatientErrors.NotFound;
        }
        patient.IsActive = request.IsActive;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// -------------------- Delete --------------------
public sealed record DeletePatientCommand(Guid Id) : IRequest<Result>;

public sealed class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeletePatientCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (patient is null)
        {
            return PatientErrors.NotFound;
        }
        _db.Patients.Remove(patient);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
