namespace Arrapa.ClinicOS.Contracts.Clinic.Profile;

public sealed record ClinicProfileResponse(
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
    decimal? DefaultConsultationFee,
    string? SubscriptionPlanName,
    DateTimeOffset? SubscriptionExpiresAt);

public sealed record UpdateClinicProfileRequest(
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? LogoUrl,
    string TimeZone,
    string Locale,
    string Currency,
    decimal? DefaultConsultationFee);
