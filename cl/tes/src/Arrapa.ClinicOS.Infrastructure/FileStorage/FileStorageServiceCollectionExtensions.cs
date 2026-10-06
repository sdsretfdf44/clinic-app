using Arrapa.ClinicOS.Application.Abstractions.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arrapa.ClinicOS.Infrastructure.FileStorage;

/// <summary>
/// Registers the file storage abstraction. Provider is selected from <c>FileStorage:Provider</c>:
/// <c>LocalDisk</c> (default), <c>AzureBlob</c>, <c>S3</c> — the latter two require adding the respective
/// package references and providers before enabling.
/// </summary>
public static class FileStorageServiceCollectionExtensions
{
    public static IServiceCollection AddArrapaFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName))
            .Validate(o => o.MaxUploadBytes > 0, "FileStorage MaxUploadBytes must be positive.");

        var provider = configuration.GetSection(FileStorageOptions.SectionName)[nameof(FileStorageOptions.Provider)] ?? "LocalDisk";

        if (string.Equals(provider, "LocalDisk", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IFileStorage, LocalDiskFileStorage>();
        }
        else
        {
            throw new NotSupportedException(
                $"File storage provider '{provider}' is not registered. Add the corresponding implementation and register it here.");
        }

        return services;
    }
}
