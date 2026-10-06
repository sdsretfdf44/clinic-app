using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Subscriptions;

/// <summary>
/// A subscription plan sold by Arrapa Software. Not tenant-scoped — plans are catalog data owned by the platform.
/// </summary>
public sealed class SubscriptionPlan : BaseEntity, IAggregateRoot
{
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public SubscriptionInterval Interval { get; set; } = SubscriptionInterval.Monthly;

    public decimal Price { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public int? MaxUsers { get; set; }

    public int? MaxPatients { get; set; }

    public int? MaxAppointmentsPerMonth { get; set; }

    /// <summary>
    /// JSON-serialized array of feature keys included in this plan.
    /// </summary>
    public string FeaturesJson { get; set; } = "[]";

    public bool IsActive { get; set; } = true;

    public bool IsDefault { get; set; }

    public int SortOrder { get; set; }
}
