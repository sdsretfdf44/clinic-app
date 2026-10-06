namespace Arrapa.ClinicOS.Contracts.Admin.Tenants;

public sealed record UpdateTenantRequest(
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? LogoUrl,
    string TimeZone,
    string Locale,
    string Currency);
