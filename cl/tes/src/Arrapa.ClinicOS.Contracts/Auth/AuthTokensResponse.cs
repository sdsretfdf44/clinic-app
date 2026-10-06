namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Access token / refresh token pair returned by login and refresh flows.
/// </summary>
public sealed record AuthTokensResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthUserResponse User);
