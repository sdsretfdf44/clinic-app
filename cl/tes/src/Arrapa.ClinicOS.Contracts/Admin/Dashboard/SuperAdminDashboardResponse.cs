namespace Arrapa.ClinicOS.Contracts.Admin.Dashboard;

public sealed record SuperAdminDashboardResponse(
    TenantKpisDto Tenants,
    UserKpisDto Users,
    RevenueKpisDto Revenue,
    IReadOnlyList<PlanRevenueDto> PlanRevenue,
    IReadOnlyList<RecentTenantDto> RecentTenants,
    IReadOnlyList<RecentActivityDto> RecentActivity);

public sealed record TenantKpisDto(int Total, int Active, int Trial, int Suspended, int NewThisMonth);

public sealed record UserKpisDto(int Total, int Active, int Inactive, int SuperAdmins, int ClinicAdmins);

public sealed record RevenueKpisDto(decimal MonthlyRecurring, decimal AnnualRecurring, string Currency);

public sealed record PlanRevenueDto(Guid PlanId, string PlanName, int TenantCount, decimal MonthlyRevenue);

public sealed record RecentTenantDto(Guid Id, string Name, string Slug, string Status, DateTimeOffset CreatedOn);

public sealed record RecentActivityDto(
    Guid Id,
    string EntityType,
    string EntityId,
    string Operation,
    string? PerformedByEmail,
    DateTimeOffset OccurredOn);
