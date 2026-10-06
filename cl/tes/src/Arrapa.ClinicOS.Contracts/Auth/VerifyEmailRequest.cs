namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Confirms an email address using the token emailed after registration.
/// </summary>
public sealed class VerifyEmailRequest
{
    public string TenantSlug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
