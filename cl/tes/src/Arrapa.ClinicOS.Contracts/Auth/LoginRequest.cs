namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Sign-in payload. If <see cref="TenantSlug"/> is null, the tenant is inferred from the <c>X-Tenant-Id</c> header
/// or the request host. <see cref="TwoFactorCode"/> is required only when 2FA is enabled for the user; the API
/// returns a validation error asking for it on the first submission.
/// </summary>
public sealed record LoginRequest(string Email, string Password, string? TenantSlug, string? TwoFactorCode = null);
