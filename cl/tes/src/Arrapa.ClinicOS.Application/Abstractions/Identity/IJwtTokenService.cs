using Arrapa.ClinicOS.Domain.Identity;

namespace Arrapa.ClinicOS.Application.Abstractions.Identity;

/// <summary>
/// Issues signed JWT access tokens and cryptographically strong refresh tokens.
/// </summary>
public interface IJwtTokenService
{
    Task<AccessTokenResult> GenerateAccessTokenAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);
    RefreshTokenResult GenerateRefreshToken();
}

/// <summary>
/// A newly minted JWT access token and the moment it expires (UTC).
/// </summary>
public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt, string TokenType = "Bearer");

/// <summary>
/// A newly minted opaque refresh token and the moment it expires (UTC).
/// </summary>
public sealed record RefreshTokenResult(string Token, DateTimeOffset ExpiresAt);
