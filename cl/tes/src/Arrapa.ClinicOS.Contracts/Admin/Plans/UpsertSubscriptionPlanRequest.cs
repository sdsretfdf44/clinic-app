namespace Arrapa.ClinicOS.Contracts.Admin.Plans;

public sealed record UpsertSubscriptionPlanRequest(
    string Code,
    string Name,
    string? Description,
    string Interval,
    decimal Price,
    string Currency,
    int? MaxUsers,
    int? MaxPatients,
    int? MaxAppointmentsPerMonth,
    IReadOnlyList<string> Features,
    bool IsActive,
    bool IsDefault,
    int SortOrder);
