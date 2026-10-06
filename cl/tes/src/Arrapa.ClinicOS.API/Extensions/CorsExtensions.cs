namespace Arrapa.ClinicOS.API.Extensions;

/// <summary>
/// CORS configuration for the frontend (React) origin, honoring the <c>AllowedOrigins</c> config key.
/// </summary>
public static class CorsExtensions
{
    public const string PolicyName = "ArrapaClinicOsCors";

    public static IServiceCollection AddArrapaCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "http://localhost:5174" };

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Correlation-Id");
            });
        });

        return services;
    }
}
