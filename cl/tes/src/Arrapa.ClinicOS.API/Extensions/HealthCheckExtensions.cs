using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Arrapa.ClinicOS.API.Extensions;

/// <summary>
/// Health checks exposed on <c>/health/live</c> (liveness) and <c>/health/ready</c> (readiness).
/// </summary>
public static class HealthCheckExtensions
{
    public static IServiceCollection AddArrapaHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        var builder = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.AddSqlServer(connectionString, name: "sqlserver", tags: new[] { "ready", "db" });
        }

        return services;
    }

    public static IEndpointRouteBuilder MapArrapaHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return endpoints;
    }
}
