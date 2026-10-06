using Arrapa.ClinicOS.Application.Abstractions.FileStorage;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Infrastructure.FileStorage;
using Arrapa.ClinicOS.Shared.Results;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Arrapa.ClinicOS.API.Controllers.v1;

/// <summary>
/// Generic file upload / download endpoints. Access is tenant-scoped: the tenant id is embedded in the key
/// prefix so uploads from tenant A can never resolve to a path served for tenant B.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/files")]
[Authorize]
public sealed class FilesController : ApiControllerBase
{
    private readonly IFileStorage _fileStorage;
    private readonly ITenantContext _tenantContext;
    private readonly FileStorageOptions _options;

    public FilesController(
        IFileStorage fileStorage,
        ITenantContext tenantContext,
        IOptions<FileStorageOptions> options)
    {
        _fileStorage = fileStorage;
        _tenantContext = tenantContext;
        _options = options.Value;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromForm] FileUploadForm form, CancellationToken cancellationToken)
    {
        if (form.File is null || form.File.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail(Error.Validation("file.missing", "No file was provided."), CorrelationId));
        }

        if (form.File.Length > _options.MaxUploadBytes)
        {
            return BadRequest(ApiResponse<object>.Fail(
                Error.Validation("file.too_large", $"File exceeds the {_options.MaxUploadBytes:N0}-byte limit."),
                CorrelationId));
        }

        if (_options.AllowedContentTypes.Count > 0 &&
            !_options.AllowedContentTypes.Contains(form.File.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<object>.Fail(
                Error.Validation("file.type_not_allowed", $"Content type '{form.File.ContentType}' is not allowed."),
                CorrelationId));
        }

        var tenantId = _tenantContext.TenantId?.ToString("N") ?? "shared";
        var category = SanitizeSegment(form.Category ?? "misc");
        var extension = Path.GetExtension(form.File.FileName);
        var key = $"tenants/{tenantId}/{category}/{Guid.NewGuid():N}{extension}";

        await using var stream = form.File.OpenReadStream();
        var stored = await _fileStorage.UploadAsync(
            new FileUploadRequest(key, stream, form.File.ContentType, form.File.Length),
            cancellationToken).ConfigureAwait(false);

        var response = new FileUploadResponse(stored.Key, stored.ContentType, stored.SizeBytes, stored.PublicUrl?.ToString());
        return Ok(ApiResponse<FileUploadResponse>.Ok(response, "File uploaded.", CorrelationId));
    }

    [HttpGet("{**key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(string key, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId?.ToString("N");
        if (tenantId is not null && !key.StartsWith($"tenants/{tenantId}/", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var stream = await _fileStorage.OpenReadAsync(key, cancellationToken).ConfigureAwait(false);
        if (stream is null)
        {
            return NotFound();
        }

        var contentType = GuessContentType(key);
        return File(stream, contentType, enableRangeProcessing: true);
    }

    [HttpDelete("{**key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string key, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId?.ToString("N");
        if (tenantId is not null && !key.StartsWith($"tenants/{tenantId}/", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var deleted = await _fileStorage.DeleteAsync(key, cancellationToken).ConfigureAwait(false);
        return deleted ? NoContent() : NotFound();
    }

    private static string SanitizeSegment(string value)
    {
        var cleaned = new string(value.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
        return string.IsNullOrEmpty(cleaned) ? "misc" : cleaned.ToLowerInvariant();
    }

    private static string GuessContentType(string key)
    {
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        return provider.TryGetContentType(key, out var contentType) ? contentType : "application/octet-stream";
    }
}

public sealed class FileUploadForm
{
    public IFormFile? File { get; set; }
    public string? Category { get; set; }
}

public sealed record FileUploadResponse(string Key, string ContentType, long SizeBytes, string? PublicUrl);
