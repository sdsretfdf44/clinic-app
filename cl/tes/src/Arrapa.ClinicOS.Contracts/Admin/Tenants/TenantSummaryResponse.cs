namespace Arrapa.ClinicOS.Contracts.Admin.Tenants;

/// <summary>
/// Summary row shown in the super-admin tenants list.
/// </summary>
public sealed record TenantSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    string? ContactEmail,
    string? SubscriptionPlanName,
    int UserCount,
    DateTimeOffset CreatedOn,
    DateTimeOffset? SuspendedAt);
