namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Requests a new access token by presenting a valid refresh token. The old refresh token is rotated (revoked +
/// replaced) as part of this operation.
/// </summary>
public sealed record RefreshTokenRequest(string RefreshToken);
