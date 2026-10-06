namespace Arrapa.ClinicOS.Contracts.Patients;

public sealed record PatientAllergyResponse(
    Guid Id,
    Guid PatientId,
    string Name,
    string Severity,
    string? ReactionDescription,
    DateOnly? IdentifiedOn,
    DateTimeOffset CreatedOn);

public sealed record UpsertPatientAllergyRequest(
    string Name,
    string Severity,
    string? ReactionDescription,
    DateOnly? IdentifiedOn);

public sealed record MedicalHistoryEntryResponse(
    Guid Id,
    Guid PatientId,
    string Condition,
    DateOnly? Since,
    string? Notes,
    bool IsChronic,
    bool IsResolved,
    DateTimeOffset CreatedOn);

public sealed record UpsertMedicalHistoryEntryRequest(
    string Condition,
    DateOnly? Since,
    string? Notes,
    bool IsChronic,
    bool IsResolved);

public sealed record PatientMedicationResponse(
    Guid Id,
    Guid PatientId,
    string Name,
    string? Dosage,
    string? Frequency,
    DateOnly? StartedOn,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedOn);

public sealed record UpsertPatientMedicationRequest(
    string Name,
    string? Dosage,
    string? Frequency,
    DateOnly? StartedOn,
    string? Notes,
    bool IsActive);
