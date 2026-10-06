namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Revokes the supplied refresh token. Access tokens remain valid until their natural expiry.
/// </summary>
public sealed record LogoutRequest(string RefreshToken);
