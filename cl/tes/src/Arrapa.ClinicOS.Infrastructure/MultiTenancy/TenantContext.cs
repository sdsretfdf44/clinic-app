using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Shared.Constants;

namespace Arrapa.ClinicOS.Infrastructure.MultiTenancy;

/// <summary>
/// Scoped implementation of <see cref="ITenantContext"/>. Populated by <c>TenantResolutionMiddleware</c> once per request.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? Slug { get; private set; }
    public bool IsResolved => TenantId.HasValue;
    public bool IsPlatform => TenantId == AppConstants.SystemTenants.PlatformTenantId;

    public void SetTenant(Guid tenantId, string? slug = null)
    {
        TenantId = tenantId;
        Slug = slug;
    }
}
