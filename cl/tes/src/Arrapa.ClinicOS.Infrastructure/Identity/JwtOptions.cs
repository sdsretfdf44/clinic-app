namespace Arrapa.ClinicOS.Infrastructure.Identity;

/// <summary>
/// Strongly typed JWT settings bound from <c>appsettings.json</c> section <c>Jwt</c>.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 30;
}
