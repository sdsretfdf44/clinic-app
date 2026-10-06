using Microsoft.AspNetCore.DataProtection;

namespace Arrapa.ClinicOS.API.Extensions;

/// <summary>
/// Persists ASP.NET Core Data Protection keys to a configurable directory so cookies/tokens survive
/// process restarts and scale to multiple instances. In production, mount a shared volume or
/// point <c>DataProtection:KeysDirectory</c> at a shared path (e.g., Azure Blob, S3, mounted NFS).
/// </summary>
public static class DataProtectionExtensions
{
    public static IServiceCollection AddArrapaDataProtection(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration["DataProtection:KeysDirectory"];
        var keysDirectory = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(environment.ContentRootPath, "keys")
            : configured;

        Directory.CreateDirectory(keysDirectory);

        services
            .AddDataProtection()
            .SetApplicationName("Arrapa.ClinicOS")
            .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));

        return services;
    }
}
