using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Audit;

/// <summary>
/// A single change captured by the audit interceptor. Not soft-deletable; append-only.
/// Note: this entity implements <see cref="IMustHaveTenant"/> via <see cref="BaseEntity"/>, but its records are
/// never removed and the tenant filter is bypassed by the super-admin viewer.
/// </summary>
public sealed class AuditLogEntry : BaseEntity, IAggregateRoot
{
    [MaxLength(200)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(64)]
    public string EntityId { get; set; } = string.Empty;

    public AuditOperation Operation { get; set; }

    /// <summary>
    /// JSON payload: <c>{ "before": { ... }, "after": { ... } }</c>. Sensitive fields (passwords, tokens) are redacted.
    /// </summary>
    public string? ChangesJson { get; set; }

    public Guid? PerformedByUserId { get; set; }

    [MaxLength(200)]
    public string? PerformedByEmail { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(64)]
    public string? CorrelationId { get; set; }
}
