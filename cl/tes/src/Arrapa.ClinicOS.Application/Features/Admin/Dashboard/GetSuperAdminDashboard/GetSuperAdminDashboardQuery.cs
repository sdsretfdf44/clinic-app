using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.Dashboard;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Dashboard.GetSuperAdminDashboard;

public sealed record GetSuperAdminDashboardQuery : IRequest<Result<SuperAdminDashboardResponse>>;

public sealed class GetSuperAdminDashboardQueryHandler : IRequestHandler<GetSuperAdminDashboardQuery, Result<SuperAdminDashboardResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public GetSuperAdminDashboardQueryHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<SuperAdminDashboardResponse>> Handle(GetSuperAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var tenantAggregates = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && t.Id != AppConstants.SystemTenants.PlatformTenantId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(t => t.Status == TenantStatus.Active),
                Trial = g.Count(t => t.Status == TenantStatus.Trial),
                Suspended = g.Count(t => t.Status == TenantStatus.Suspended),
                NewThisMonth = g.Count(t => t.CreatedOn >= monthStart)
            })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var tenants = new TenantKpisDto(
            tenantAggregates?.Total ?? 0,
            tenantAggregates?.Active ?? 0,
            tenantAggregates?.Trial ?? 0,
            tenantAggregates?.Suspended ?? 0,
            tenantAggregates?.NewThisMonth ?? 0);

        var totalUsers = await _db.Users.AsNoTracking().IgnoreQueryFilters().CountAsync(u => !u.IsDeleted, cancellationToken).ConfigureAwait(false);
        var activeUsers = await _db.Users.AsNoTracking().IgnoreQueryFilters().CountAsync(u => !u.IsDeleted && u.IsActive, cancellationToken).ConfigureAwait(false);
        var superAdmins = await _db.Users.AsNoTracking().IgnoreQueryFilters().CountAsync(u => !u.IsDeleted && u.PrimaryRole == UserRole.SuperAdmin, cancellationToken).ConfigureAwait(false);
        var clinicAdmins = await _db.Users.AsNoTracking().IgnoreQueryFilters().CountAsync(u => !u.IsDeleted && u.PrimaryRole == UserRole.ClinicAdmin, cancellationToken).ConfigureAwait(false);

        var users = new UserKpisDto(totalUsers, activeUsers, totalUsers - activeUsers, superAdmins, clinicAdmins);

        var planBreakdown = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && t.SubscriptionPlanId != null && t.Status == TenantStatus.Active)
            .Join(_db.SubscriptionPlans.IgnoreQueryFilters().Where(p => !p.IsDeleted),
                t => t.SubscriptionPlanId, p => p.Id, (t, p) => new { Tenant = t, Plan = p })
            .GroupBy(x => new { x.Plan.Id, x.Plan.Name, x.Plan.Price, x.Plan.Interval })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Name,
                g.Key.Price,
                g.Key.Interval,
                TenantCount = g.Count()
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        decimal ToMonthlyRevenue(decimal price, SubscriptionInterval interval)
            => interval == SubscriptionInterval.Annual ? price / 12m : price;

        var planRevenue = planBreakdown.Select(p => new PlanRevenueDto(
            p.Id, p.Name, p.TenantCount, ToMonthlyRevenue(p.Price, p.Interval) * p.TenantCount)).ToList();

        var mrr = planRevenue.Sum(p => p.MonthlyRevenue);
        var arr = mrr * 12m;
        var currency = await _db.SubscriptionPlans.AsNoTracking().IgnoreQueryFilters()
            .Where(p => !p.IsDeleted).Select(p => p.Currency).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "USD";
        var revenue = new RevenueKpisDto(mrr, arr, currency);

        var recentTenants = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && t.Id != AppConstants.SystemTenants.PlatformTenantId)
            .OrderByDescending(t => t.CreatedOn)
            .Take(5)
            .Select(t => new RecentTenantDto(t.Id, t.Name, t.Slug, t.Status.ToString(), t.CreatedOn))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var recentActivity = await _db.AuditLogs.AsNoTracking().IgnoreQueryFilters()
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedOn)
            .Take(10)
            .Select(a => new RecentActivityDto(
                a.Id, a.EntityType, a.EntityId, a.Operation.ToString(), a.PerformedByEmail, a.CreatedOn))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new SuperAdminDashboardResponse(tenants, users, revenue, planRevenue, recentTenants, recentActivity);
    }
}
