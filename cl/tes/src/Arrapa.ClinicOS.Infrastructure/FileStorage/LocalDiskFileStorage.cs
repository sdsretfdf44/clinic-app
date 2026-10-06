using Arrapa.ClinicOS.Application.Abstractions.FileStorage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arrapa.ClinicOS.Infrastructure.FileStorage;

/// <summary>
/// Local filesystem file store. Intended for local dev and small single-instance deployments.
/// For any multi-instance deployment use Azure Blob or S3.
/// </summary>
public sealed class LocalDiskFileStorage : IFileStorage
{
    private readonly FileStorageOptions _options;
    private readonly ILogger<LocalDiskFileStorage> _logger;
    private readonly string _rootPath;

    public LocalDiskFileStorage(
        IOptions<FileStorageOptions> options,
        IHostEnvironment environment,
        ILogger<LocalDiskFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _rootPath = string.IsNullOrWhiteSpace(_options.LocalRootPath)
            ? Path.Combine(environment.ContentRootPath, "uploads")
            : _options.LocalRootPath;

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredFile> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        ValidateKey(request.Key);

        var target = ResolvePath(request.Key);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);

        await using var file = File.Create(target);
        await request.Content.CopyToAsync(file, cancellationToken).ConfigureAwait(false);

        var size = file.Length;
        _logger.LogInformation("File stored at {Path} size={Size} type={ContentType}", target, size, request.ContentType);

        return new StoredFile(request.Key, request.ContentType, size, BuildPublicUrl(request.Key));
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        var path = ResolvePath(key);
        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = File.OpenRead(path);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        var path = ResolvePath(key);
        if (!File.Exists(path))
        {
            return Task.FromResult(false);
        }

        File.Delete(path);
        return Task.FromResult(true);
    }

    public Task<Uri?> GetPublicUrlAsync(string key, TimeSpan? validFor = null, CancellationToken cancellationToken = default)
        => Task.FromResult(BuildPublicUrl(key));

    private Uri? BuildPublicUrl(string key)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            return null;
        }

        var basePath = _options.PublicBaseUrl.TrimEnd('/');
        var relative = $"{basePath}/{key.TrimStart('/')}";

        if (Uri.TryCreate(relative, UriKind.Absolute, out var absolute))
        {
            return absolute;
        }

        return new Uri(relative, UriKind.Relative);
    }

    private string ResolvePath(string key)
    {
        var normalized = key.Replace('\\', '/').TrimStart('/');
        var combined = Path.GetFullPath(Path.Combine(_rootPath, normalized));

        if (!combined.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Resolved path escapes the storage root.");
        }

        return combined;
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Storage key must not be empty.", nameof(key));
        }

        if (key.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException("Storage key must not contain traversal segments.", nameof(key));
        }
    }
}
