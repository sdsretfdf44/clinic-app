namespace Arrapa.ClinicOS.Contracts.Auth;

public sealed class ResendVerificationEmailRequest
{
    public string TenantSlug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
