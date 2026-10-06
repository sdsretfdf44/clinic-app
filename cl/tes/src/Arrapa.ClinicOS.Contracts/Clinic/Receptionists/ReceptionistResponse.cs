namespace Arrapa.ClinicOS.Contracts.Clinic.Receptionists;

public sealed record ReceptionistResponse(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    string? Title,
    bool IsActive,
    int SortOrder,
    DateTimeOffset CreatedOn);

public sealed record CreateReceptionistRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? Title);

public sealed record UpdateReceptionistRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl,
    string? Title,
    int SortOrder);
