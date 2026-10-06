namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Payload for starting 2FA enrollment: server returns the shared secret + otpauth URI to render as a QR.
/// </summary>
public sealed record TwoFactorSetupResponse(string SharedKey, string AuthenticatorUri);

public sealed class EnableTwoFactorRequest
{
    public string VerificationCode { get; set; } = string.Empty;
}

public sealed class DisableTwoFactorRequest
{
    public string VerificationCode { get; set; } = string.Empty;
}

public sealed record TwoFactorStatusResponse(bool Enabled);
