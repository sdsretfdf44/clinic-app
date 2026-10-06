using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.FeatureFlags;

/// <summary>
/// Per-tenant override for a <see cref="FeatureFlag"/>. Absence of an override means the flag's default applies.
/// </summary>
public sealed class TenantFeatureFlag : BaseEntity
{
    public Guid FeatureFlagId { get; set; }
    public FeatureFlag? FeatureFlag { get; set; }

    public bool IsEnabled { get; set; }
}
