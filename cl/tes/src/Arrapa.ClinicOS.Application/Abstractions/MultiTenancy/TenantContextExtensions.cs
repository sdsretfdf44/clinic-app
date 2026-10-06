using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;

/// <summary>
/// Small helpers for enforcing tenant scope inside clinic-admin handlers.
/// </summary>
public static class TenantContextExtensions
{
    public static Result<Guid> RequireTenantId(this ITenantContext tenantContext)
    {
        if (tenantContext.TenantId is Guid id && id != Guid.Empty)
        {
            return id;
        }
        return ClinicErrors.NoTenantContext;
    }
}
