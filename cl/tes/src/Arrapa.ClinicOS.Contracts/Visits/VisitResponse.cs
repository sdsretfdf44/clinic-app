namespace Arrapa.ClinicOS.Contracts.Visits;

public sealed record VisitDetailResponse(
    Guid Id,
    Guid? AppointmentId,
    string? AppointmentNumber,
    Guid PatientId,
    string PatientName,
    string PatientMrn,
    int PatientAge,
    Guid DoctorId,
    string DoctorName,
    string? DoctorSpecialty,
    DateTimeOffset VisitDate,
    string? Symptoms,
    string? Diagnosis,
    string? DoctorNotes,
    bool IsLocked,
    VitalsResponse? Vitals,
    IReadOnlyList<PrescriptionResponse> Prescriptions,
    DateTimeOffset CreatedOn,
    DateTimeOffset? UpdatedOn);

public sealed record VitalsResponse(
    Guid Id,
    int? BloodPressureSystolic,
    int? BloodPressureDiastolic,
    int? PulseBpm,
    decimal? TemperatureCelsius,
    decimal? HeightCm,
    decimal? WeightKg,
    decimal? Bmi,
    int? SpO2Percent,
    int? RespirationRate,
    string? Notes);

public sealed record UpsertVitalsRequest(
    int? BloodPressureSystolic,
    int? BloodPressureDiastolic,
    int? PulseBpm,
    decimal? TemperatureCelsius,
    decimal? HeightCm,
    decimal? WeightKg,
    int? SpO2Percent,
    int? RespirationRate,
    string? Notes);

public sealed record UpsertDiagnosisRequest(
    string? Symptoms,
    string? Diagnosis,
    string? DoctorNotes);

public sealed record PrescriptionResponse(
    Guid Id,
    DateTimeOffset IssuedOn,
    string? GeneralInstructions,
    IReadOnlyList<PrescriptionItemResponse> Items);

public sealed record PrescriptionItemResponse(
    Guid Id,
    string Medicine,
    string? Strength,
    string? Dosage,
    string? Frequency,
    string? Duration,
    string? Instructions,
    int SortOrder);

public sealed record UpsertPrescriptionRequest(
    string? GeneralInstructions,
    IReadOnlyList<UpsertPrescriptionItemRequest> Items);

public sealed record UpsertPrescriptionItemRequest(
    string Medicine,
    string? Strength,
    string? Dosage,
    string? Frequency,
    string? Duration,
    string? Instructions,
    int SortOrder);
