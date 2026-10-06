namespace Arrapa.ClinicOS.Application.Abstractions.FileStorage;

/// <summary>
/// Abstraction over blob storage. Implementations: local disk (dev), Azure Blob, AWS S3.
/// Keys are provider-agnostic paths using forward slashes (e.g., <c>tenants/acme/patients/123/avatar.png</c>).
/// </summary>
public interface IFileStorage
{
    Task<StoredFile> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

    Task<Uri?> GetPublicUrlAsync(string key, TimeSpan? validFor = null, CancellationToken cancellationToken = default);
}

public sealed record FileUploadRequest(
    string Key,
    Stream Content,
    string ContentType,
    long? ContentLength = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record StoredFile(
    string Key,
    string ContentType,
    long SizeBytes,
    Uri? PublicUrl);
