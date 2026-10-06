using Asp.Versioning;

namespace Arrapa.ClinicOS.API.Extensions;

/// <summary>
/// Registers URL-segment API versioning (e.g., <c>/api/v1/...</c>) with sensible defaults.
/// </summary>
public static class ApiVersioningExtensions
{
    public static IServiceCollection AddArrapaApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        return services;
    }
}
