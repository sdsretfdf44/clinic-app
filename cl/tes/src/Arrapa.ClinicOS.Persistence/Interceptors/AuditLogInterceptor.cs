using System.Text.Json;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Domain.Audit;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Time;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Arrapa.ClinicOS.Persistence.Interceptors;

/// <summary>
/// Captures a durable audit log entry for every mutation to a persisted <see cref="BaseEntity"/>. Runs before save
/// so we can diff the entity's original vs. current values; entries are appended to the change tracker and saved
/// alongside the underlying operation in the same transaction.
/// </summary>
public sealed class AuditLogInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> RedactedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "Token",
        "RefreshToken", "SigningKey", "AccessFailedCount", "RowVersion"
    };

    private static readonly HashSet<string> IgnoredEntityTypes = new(StringComparer.Ordinal)
    {
        nameof(AuditLogEntry)
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogInterceptor(
        ICurrentUserService currentUser,
        ITenantContext tenantContext,
        IDateTimeProvider clock,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _clock = clock;
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Capture(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => !IgnoredEntityTypes.Contains(e.Metadata.ClrType.Name))
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var ctx = _httpContextAccessor.HttpContext;
        var ipAddress = ctx?.Connection.RemoteIpAddress?.ToString();
        var userAgent = ctx?.Request.Headers.UserAgent.ToString();
        var correlationId = ctx?.Items[AppConstants.Http.CorrelationIdHeader]?.ToString();
        var now = _clock.UtcNow;
        var userId = _currentUser.UserId;
        var email = _currentUser.Email;

        foreach (var entry in entries)
        {
            var operation = entry.State switch
            {
                EntityState.Added => AuditOperation.Created,
                EntityState.Deleted => AuditOperation.Deleted,
                _ => AuditOperation.Updated
            };

            // Skip soft-delete "delete" transitions — they are represented as Updated by the audit interceptor upstream.
            if (entry.State == EntityState.Modified && operation == AuditOperation.Updated)
            {
                var changed = entry.Properties.Where(p => p.IsModified).ToList();
                if (changed.Count == 0)
                {
                    continue;
                }
            }

            var changesJson = SerializeChanges(entry, operation);
            var tenantId = entry.Entity.TenantId != Guid.Empty
                ? entry.Entity.TenantId
                : _tenantContext.TenantId ?? Guid.Empty;

            var log = new AuditLogEntry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = entry.Entity.Id.ToString(),
                Operation = operation,
                ChangesJson = changesJson,
                PerformedByUserId = userId,
                PerformedByEmail = email,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CorrelationId = correlationId,
                CreatedOn = now
            };

            context.Add(log);
        }
    }

    private static string? SerializeChanges(EntityEntry<BaseEntity> entry, AuditOperation operation)
    {
        var payload = new Dictionary<string, object?>(StringComparer.Ordinal);

        switch (operation)
        {
            case AuditOperation.Created:
            {
                var after = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var p in entry.Properties)
                {
                    if (RedactedProperties.Contains(p.Metadata.Name))
                    {
                        continue;
                    }
                    after[p.Metadata.Name] = p.CurrentValue;
                }
                payload["after"] = after;
                break;
            }
            case AuditOperation.Deleted:
            {
                var before = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var p in entry.Properties)
                {
                    if (RedactedProperties.Contains(p.Metadata.Name))
                    {
                        continue;
                    }
                    before[p.Metadata.Name] = p.OriginalValue;
                }
                payload["before"] = before;
                break;
            }
            default:
            {
                var before = new Dictionary<string, object?>(StringComparer.Ordinal);
                var after = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var p in entry.Properties.Where(x => x.IsModified))
                {
                    if (RedactedProperties.Contains(p.Metadata.Name))
                    {
                        continue;
                    }
                    before[p.Metadata.Name] = p.OriginalValue;
                    after[p.Metadata.Name] = p.CurrentValue;
                }
                if (before.Count == 0 && after.Count == 0)
                {
                    return null;
                }
                payload["before"] = before;
                payload["after"] = after;
                break;
            }
        }

        return JsonSerializer.Serialize(payload, JsonOptions);
    }
}
