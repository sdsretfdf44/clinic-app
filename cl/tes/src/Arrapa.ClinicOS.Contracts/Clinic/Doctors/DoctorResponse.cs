namespace Arrapa.ClinicOS.Contracts.Clinic.Doctors;

public sealed record DoctorSummaryResponse(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    string? Specialty,
    Guid? DepartmentId,
    string? DepartmentName,
    decimal? EffectiveConsultationFee,
    string Currency,
    bool IsActive,
    bool IsAcceptingNewPatients,
    DateTimeOffset CreatedOn);

public sealed record DoctorDetailResponse(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    string? Specialty,
    string? LicenseNumber,
    string? RegistrationNumber,
    Guid? DepartmentId,
    string? DepartmentName,
    decimal? ConsultationFee,
    decimal? EffectiveConsultationFee,
    string Currency,
    string? Bio,
    bool IsAcceptingNewPatients,
    bool IsActive,
    int SortOrder,
    DateTimeOffset CreatedOn,
    DateTimeOffset? UpdatedOn);

public sealed record CreateDoctorRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? Specialty,
    string? LicenseNumber,
    string? RegistrationNumber,
    Guid? DepartmentId,
    decimal? ConsultationFee,
    string? Bio,
    bool IsAcceptingNewPatients);

public sealed record UpdateDoctorRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl,
    string? Specialty,
    string? LicenseNumber,
    string? RegistrationNumber,
    Guid? DepartmentId,
    decimal? ConsultationFee,
    string? Bio,
    bool IsAcceptingNewPatients,
    int SortOrder);
