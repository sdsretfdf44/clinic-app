using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Arrapa.ClinicOS.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that applies audit stamps, soft-delete semantics, and tenant isolation on every write.
/// This is the single write-side enforcement point for all cross-cutting persistence concerns.
/// </summary>
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditableEntitySaveChangesInterceptor(
        ICurrentUserService currentUser,
        ITenantContext tenantContext,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditAndTenant(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditAndTenant(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditAndTenant(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = _dateTimeProvider.UtcNow;
        var userId = _currentUser.UserId;
        var tenantId = _tenantContext.TenantId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            ApplyToEntry(entry, now, userId, tenantId);
        }
    }

    private static void ApplyToEntry(EntityEntry entry, DateTimeOffset now, Guid? userId, Guid? tenantId)
    {
        if (entry.Entity is IMustHaveTenant tenantScoped && entry.State == EntityState.Added && tenantScoped.TenantId == Guid.Empty && tenantId is not null)
        {
            tenantScoped.TenantId = tenantId.Value;
        }

        if (entry.Entity is IAuditable auditable)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    auditable.CreatedOn = now;
                    auditable.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    auditable.UpdatedOn = now;
                    auditable.UpdatedBy = userId;
                    break;
            }
        }

        if (entry.Entity is ISoftDelete softDelete && entry.State == EntityState.Deleted)
        {
            entry.State = EntityState.Modified;
            softDelete.IsDeleted = true;
            softDelete.DeletedOn = now;
            softDelete.DeletedBy = userId;
        }
    }
}
