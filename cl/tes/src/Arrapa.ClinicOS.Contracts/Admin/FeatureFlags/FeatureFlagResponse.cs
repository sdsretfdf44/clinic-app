namespace Arrapa.ClinicOS.Contracts.Admin.FeatureFlags;

public sealed record FeatureFlagResponse(
    Guid Id,
    string Key,
    string Name,
    string? Description,
    bool DefaultEnabled,
    bool IsGlobal,
    int OverrideCount);

public sealed record TenantFeatureOverrideResponse(
    Guid Id,
    Guid TenantId,
    string TenantName,
    Guid FeatureFlagId,
    string FeatureKey,
    bool IsEnabled);
