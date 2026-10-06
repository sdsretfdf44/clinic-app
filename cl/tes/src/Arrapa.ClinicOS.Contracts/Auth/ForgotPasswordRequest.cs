namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Initiates a password reset flow. The response is always success to avoid revealing whether an account exists.
/// </summary>
public sealed record ForgotPasswordRequest(string Email, string? TenantSlug);
