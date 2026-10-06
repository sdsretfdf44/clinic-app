namespace Arrapa.ClinicOS.Contracts.Admin.Tenants;

/// <summary>
/// Payload to provision a new tenant plus its initial clinic-admin user.
/// </summary>
public sealed record CreateTenantRequest(
    string Name,
    string Slug,
    string? ContactEmail,
    string? ContactPhone,
    string TimeZone,
    string Locale,
    string Currency,
    Guid? SubscriptionPlanId,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail);
