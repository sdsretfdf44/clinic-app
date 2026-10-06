namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// User principal information returned to the client on login and via <c>/auth/me</c>.
/// </summary>
public sealed record AuthUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? AvatarUrl,
    string? PhoneNumber,
    Guid TenantId,
    string TenantSlug,
    string TenantName,
    string PrimaryRole,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset? LastLoginAt);
