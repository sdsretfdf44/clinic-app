using System.Text.Json;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.Plans;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Plans.ListPlans;

public sealed record ListPlansQuery(bool IncludeInactive) : IRequest<Result<IReadOnlyList<SubscriptionPlanResponse>>>;

public sealed class ListPlansQueryHandler : IRequestHandler<ListPlansQuery, Result<IReadOnlyList<SubscriptionPlanResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListPlansQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<SubscriptionPlanResponse>>> Handle(ListPlansQuery request, CancellationToken cancellationToken)
    {
        var query = _db.SubscriptionPlans.AsNoTracking().IgnoreQueryFilters().Where(p => !p.IsDeleted);
        if (!request.IncludeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        var plans = await query.OrderBy(p => p.SortOrder).ThenBy(p => p.Price).ToListAsync(cancellationToken).ConfigureAwait(false);
        var planIds = plans.Select(p => p.Id).ToList();

        var tenantCounts = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => !t.IsDeleted && t.SubscriptionPlanId != null && planIds.Contains(t.SubscriptionPlanId!.Value))
            .GroupBy(t => t.SubscriptionPlanId!.Value)
            .Select(g => new { PlanId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PlanId, x => x.Count, cancellationToken).ConfigureAwait(false);

        var items = plans.Select(p => new SubscriptionPlanResponse(
            p.Id,
            p.Code,
            p.Name,
            p.Description,
            p.Interval.ToString(),
            p.Price,
            p.Currency,
            p.MaxUsers,
            p.MaxPatients,
            p.MaxAppointmentsPerMonth,
            ParseFeatures(p.FeaturesJson),
            p.IsActive,
            p.IsDefault,
            p.SortOrder,
            tenantCounts.GetValueOrDefault(p.Id, 0))).ToList();

        return items;
    }

    private static IReadOnlyList<string> ParseFeatures(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }
}
