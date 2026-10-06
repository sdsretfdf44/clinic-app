using Arrapa.ClinicOS.Application.Abstractions.FeatureFlags;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Infrastructure.FeatureFlags;

/// <summary>
/// Scoped feature-flag evaluator with a per-request cache. For a flag marked <c>IsGlobal</c>, the platform default
/// applies to every tenant; otherwise the tenant's override is consulted first, falling back to the default.
/// </summary>
public sealed class FeatureFlagEvaluator : IFeatureFlagEvaluator
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly Dictionary<(string key, Guid tenantId), bool> _cache = new();

    public FeatureFlagEvaluator(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId ?? Guid.Empty;
        return IsEnabledForTenantAsync(key, tenantId, cancellationToken);
    }

    public async Task<bool> IsEnabledForTenantAsync(string key, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var normalized = key.Trim().ToLowerInvariant();
        var cacheKey = (normalized, tenantId);
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var flag = await _db.FeatureFlags.AsNoTracking().IgnoreQueryFilters()
            .Where(f => f.Key == normalized && !f.IsDeleted)
            .Select(f => new { f.Id, f.DefaultEnabled, f.IsGlobal })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (flag is null)
        {
            _cache[cacheKey] = false;
            return false;
        }

        if (flag.IsGlobal || tenantId == Guid.Empty)
        {
            _cache[cacheKey] = flag.DefaultEnabled;
            return flag.DefaultEnabled;
        }

        var overrideValue = await _db.TenantFeatureFlags.AsNoTracking().IgnoreQueryFilters()
            .Where(o => o.FeatureFlagId == flag.Id && o.TenantId == tenantId && !o.IsDeleted)
            .Select(o => (bool?)o.IsEnabled)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var enabled = overrideValue ?? flag.DefaultEnabled;
        _cache[cacheKey] = enabled;
        return enabled;
    }
}
