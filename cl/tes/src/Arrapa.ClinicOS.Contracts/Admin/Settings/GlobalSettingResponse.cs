namespace Arrapa.ClinicOS.Contracts.Admin.Settings;

public sealed record GlobalSettingResponse(
    Guid Id,
    string Key,
    string Value,
    string Category,
    string DataType,
    string? Description,
    bool IsSecret,
    DateTimeOffset? UpdatedOn);

public sealed record UpsertSettingRequest(
    string Key,
    string Value,
    string Category,
    string DataType,
    string? Description,
    bool IsSecret);
