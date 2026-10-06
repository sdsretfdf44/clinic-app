using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Tenants;

/// <summary>
/// A tenant (clinic organization) in the SaaS platform.
/// Note: Tenant itself is <em>not</em> tenant-scoped (its <see cref="BaseEntity.TenantId"/> always equals its own <c>Id</c>).
/// </summary>
public sealed class Tenant : BaseEntity, IAggregateRoot
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-safe identifier used for subdomain-based tenant resolution.
    /// </summary>
    [MaxLength(64)]
    public string Slug { get; set; } = string.Empty;

    public TenantStatus Status { get; set; } = TenantStatus.Trial;

    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(100)]
    public string TimeZone { get; set; } = "UTC";

    [MaxLength(10)]
    public string Locale { get; set; } = "en-US";

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public DateTimeOffset? SubscriptionExpiresAt { get; set; }

    public Guid? SubscriptionPlanId { get; set; }

    /// <summary>
    /// Clinic-wide default consultation fee. Overridden by <see cref="Clinical.Department.DefaultConsultationFee"/>
    /// and further overridden by <see cref="Clinical.Doctor.ConsultationFee"/>.
    /// </summary>
    public decimal? DefaultConsultationFee { get; set; }

    public DateTimeOffset? SuspendedAt { get; set; }

    [MaxLength(500)]
    public string? SuspensionReason { get; set; }
}
