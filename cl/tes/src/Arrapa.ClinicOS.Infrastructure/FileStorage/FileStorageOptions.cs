namespace Arrapa.ClinicOS.Infrastructure.FileStorage;

/// <summary>
/// Configuration bound from <c>FileStorage</c> section. Provider selects the implementation:
/// <c>LocalDisk</c> (default, dev friendly), <c>AzureBlob</c>, or <c>S3</c>.
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string Provider { get; set; } = "LocalDisk";

    /// <summary>Absolute path for the LocalDisk provider. Empty defaults to <c>{ContentRoot}/uploads</c>.</summary>
    public string LocalRootPath { get; set; } = string.Empty;

    /// <summary>Base URL where uploaded files are served from (LocalDisk: relative to the API host).</summary>
    public string PublicBaseUrl { get; set; } = "/files";

    /// <summary>Hard limit on individual upload size. Defaults to 10 MB.</summary>
    public long MaxUploadBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Whitelist of allowed content types. Empty = allow anything (not recommended in production).</summary>
    public List<string> AllowedContentTypes { get; set; } = new();
}
