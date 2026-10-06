using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.FeatureFlags;

/// <summary>
/// Global feature flag definition. Each flag has a platform-wide default that individual tenants may override.
/// </summary>
public sealed class FeatureFlag : BaseEntity, IAggregateRoot
{
    [MaxLength(128)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool DefaultEnabled { get; set; }

    /// <summary>
    /// If true the flag applies uniformly to every tenant and per-tenant overrides are ignored.
    /// </summary>
    public bool IsGlobal { get; set; }

    public ICollection<TenantFeatureFlag> TenantOverrides { get; set; } = new List<TenantFeatureFlag>();
}
