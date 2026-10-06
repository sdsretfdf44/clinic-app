using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.FeatureFlags;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.FeatureFlags.ListFeatureFlags;

public sealed record ListFeatureFlagsQuery : IRequest<Result<IReadOnlyList<FeatureFlagResponse>>>;

public sealed class ListFeatureFlagsQueryHandler : IRequestHandler<ListFeatureFlagsQuery, Result<IReadOnlyList<FeatureFlagResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListFeatureFlagsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<FeatureFlagResponse>>> Handle(ListFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        var flags = await _db.FeatureFlags.AsNoTracking().IgnoreQueryFilters()
            .Where(f => !f.IsDeleted)
            .OrderBy(f => f.Key)
            .Select(f => new FeatureFlagResponse(
                f.Id, f.Key, f.Name, f.Description, f.DefaultEnabled, f.IsGlobal,
                _db.TenantFeatureFlags.IgnoreQueryFilters().Count(o => o.FeatureFlagId == f.Id && !o.IsDeleted)))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return flags;
    }
}
