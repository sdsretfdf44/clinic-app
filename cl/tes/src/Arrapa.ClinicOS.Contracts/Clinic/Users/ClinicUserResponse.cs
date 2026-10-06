namespace Arrapa.ClinicOS.Contracts.Clinic.Users;

public sealed record ClinicUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? PhoneNumber,
    string PrimaryRole,
    IReadOnlyList<string> Roles,
    bool IsActive,
    bool IsLockedOut,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedOn);

public sealed record InviteUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string PrimaryRole);

public sealed record ChangeUserRoleRequest(string PrimaryRole);
