namespace Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;

/// <summary>
/// Ambient tenant context for the current request or background job.
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
    string? Slug { get; }
    bool IsResolved { get; }
    bool IsPlatform { get; }
    void SetTenant(Guid tenantId, string? slug = null);
}
