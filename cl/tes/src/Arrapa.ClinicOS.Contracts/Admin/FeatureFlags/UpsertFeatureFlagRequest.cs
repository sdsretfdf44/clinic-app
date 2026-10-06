namespace Arrapa.ClinicOS.Contracts.Admin.FeatureFlags;

public sealed record UpsertFeatureFlagRequest(
    string Key,
    string Name,
    string? Description,
    bool DefaultEnabled,
    bool IsGlobal);

public sealed record SetFeatureOverrideRequest(Guid TenantId, bool IsEnabled);
