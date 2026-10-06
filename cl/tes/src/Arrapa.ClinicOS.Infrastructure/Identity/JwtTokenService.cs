using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Time;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Arrapa.ClinicOS.Infrastructure.Identity;

/// <summary>
/// Produces signed JWT access tokens and cryptographically strong refresh tokens.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _clock;

    public JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public Task<AccessTokenResult> GenerateAccessTokenAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var expires = _clock.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppConstants.Claims.UserId, user.Id.ToString()),
            new(AppConstants.Claims.Email, user.Email ?? string.Empty),
            new(AppConstants.Claims.FullName, user.FullName),
            new(AppConstants.Claims.TenantId, user.TenantId.ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _clock.UtcNow.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(new AccessTokenResult(jwt, expires));
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var expiresAt = _clock.UtcNow.AddDays(_options.RefreshTokenDays);
        return new RefreshTokenResult(token, expiresAt);
    }
}
