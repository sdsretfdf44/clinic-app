using Arrapa.ClinicOS.Domain.Tenants;

namespace Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;

/// <summary>
/// Resolves a tenant by slug or id, bypassing the tenant scope of the current request. Used by the auth pipeline
/// (before a user is authenticated) and by super-admin flows.
/// </summary>
public interface ITenantLookupService
{
    Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Tenant?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
