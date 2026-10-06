namespace Arrapa.ClinicOS.Contracts.Clinic.Departments;

public sealed record DepartmentResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal? DefaultConsultationFee,
    bool IsActive,
    int SortOrder,
    int DoctorCount,
    DateTimeOffset CreatedOn);

public sealed record UpsertDepartmentRequest(
    string Name,
    string? Description,
    decimal? DefaultConsultationFee,
    bool IsActive,
    int SortOrder);
