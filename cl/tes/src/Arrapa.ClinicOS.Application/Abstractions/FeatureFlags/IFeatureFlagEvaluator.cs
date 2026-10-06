namespace Arrapa.ClinicOS.Application.Abstractions.FeatureFlags;

/// <summary>
/// Evaluates whether a feature flag is enabled for the current tenant. Consumers depend on this contract; the
/// implementation caches lookups per request.
/// </summary>
public interface IFeatureFlagEvaluator
{
    Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> IsEnabledForTenantAsync(string key, Guid tenantId, CancellationToken cancellationToken = default);
}
