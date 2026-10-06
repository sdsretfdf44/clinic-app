namespace Arrapa.ClinicOS.Contracts.Clinic.Settings;

public sealed record ClinicSettingResponse(
    Guid Id,
    string Key,
    string Value,
    string Category,
    string DataType,
    string? Description,
    bool IsSecret,
    DateTimeOffset? UpdatedOn);

public sealed record UpsertClinicSettingRequest(
    string Key,
    string Value,
    string Category,
    string DataType,
    string? Description,
    bool IsSecret);
