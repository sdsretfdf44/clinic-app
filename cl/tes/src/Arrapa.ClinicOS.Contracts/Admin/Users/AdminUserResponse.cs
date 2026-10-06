namespace Arrapa.ClinicOS.Contracts.Admin.Users;

public sealed record AdminUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? PhoneNumber,
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    string PrimaryRole,
    IReadOnlyList<string> Roles,
    bool IsActive,
    bool IsLockedOut,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedOn);
