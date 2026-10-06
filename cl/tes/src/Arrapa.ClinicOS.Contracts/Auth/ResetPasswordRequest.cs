namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Completes a password reset using the token delivered to the user's email inbox.
/// </summary>
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword, string? TenantSlug);
