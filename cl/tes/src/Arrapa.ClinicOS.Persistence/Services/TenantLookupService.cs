using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Domain.Tenants;
using Arrapa.ClinicOS.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Arrapa.ClinicOS.Persistence.Services;

/// <summary>
/// EF Core implementation of <see cref="ITenantLookupService"/> backed by an in-process memory cache.
/// </summary>
public sealed class TenantLookupService : ITenantLookupService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public TenantLookupService(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var normalized = slug.Trim().ToLowerInvariant();
        var cacheKey = $"tenant:slug:{normalized}";

        if (_cache.TryGetValue(cacheKey, out Tenant? cached) && cached is not null)
        {
            return cached;
        }

        var tenant = await _db.Tenants
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == normalized && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (tenant is not null)
        {
            _cache.Set(cacheKey, tenant, CacheDuration);
        }

        return tenant;
    }

    public async Task<Tenant?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var cacheKey = $"tenant:id:{id}";
        if (_cache.TryGetValue(cacheKey, out Tenant? cached) && cached is not null)
        {
            return cached;
        }

        var tenant = await _db.Tenants
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (tenant is not null)
        {
            _cache.Set(cacheKey, tenant, CacheDuration);
        }

        return tenant;
    }
}
