using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.ListTenants;

public sealed class ListTenantsQueryHandler : IRequestHandler<ListTenantsQuery, Result<PaginatedList<TenantSummaryResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListTenantsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<PaginatedList<TenantSummaryResponse>>> Handle(ListTenantsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Tenants.AsNoTracking().IgnoreQueryFilters().Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(term) || t.Slug.Contains(term) || (t.ContactEmail != null && t.ContactEmail.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TenantStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        query = (request.SortBy?.ToLowerInvariant(), request.SortDirection) switch
        {
            ("name", SortDirection.Descending) => query.OrderByDescending(t => t.Name),
            ("name", _) => query.OrderBy(t => t.Name),
            ("status", SortDirection.Descending) => query.OrderByDescending(t => t.Status),
            ("status", _) => query.OrderBy(t => t.Status),
            (_, SortDirection.Ascending) => query.OrderBy(t => t.CreatedOn),
            _ => query.OrderByDescending(t => t.CreatedOn)
        };

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var page = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new
            {
                Tenant = t,
                PlanName = _db.SubscriptionPlans.IgnoreQueryFilters().Where(p => p.Id == t.SubscriptionPlanId).Select(p => p.Name).FirstOrDefault(),
                UserCount = _db.Users.IgnoreQueryFilters().Count(u => u.TenantId == t.Id && !u.IsDeleted)
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var items = page.Select(row => new TenantSummaryResponse(
            row.Tenant.Id,
            row.Tenant.Name,
            row.Tenant.Slug,
            row.Tenant.Status.ToString(),
            row.Tenant.ContactEmail,
            row.PlanName,
            row.UserCount,
            row.Tenant.CreatedOn,
            row.Tenant.SuspendedAt)).ToList();

        return new PaginatedList<TenantSummaryResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
