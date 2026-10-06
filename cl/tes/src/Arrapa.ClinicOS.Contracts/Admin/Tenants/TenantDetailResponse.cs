namespace Arrapa.ClinicOS.Contracts.Admin.Tenants;

/// <summary>
/// Full tenant detail returned by <c>GET /admin/tenants/{id}</c>.
/// </summary>
public sealed record TenantDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    string? ContactEmail,
    string? ContactPhone,
    string? LogoUrl,
    string TimeZone,
    string Locale,
    string Currency,
    Guid? SubscriptionPlanId,
    string? SubscriptionPlanName,
    DateTimeOffset? SubscriptionExpiresAt,
    DateTimeOffset? SuspendedAt,
    string? SuspensionReason,
    int UserCount,
    DateTimeOffset CreatedOn,
    DateTimeOffset? UpdatedOn);
