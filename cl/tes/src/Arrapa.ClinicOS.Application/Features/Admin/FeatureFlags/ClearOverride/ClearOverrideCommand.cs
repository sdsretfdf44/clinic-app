using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.FeatureFlags.ClearOverride;

public sealed record ClearOverrideCommand(Guid FeatureFlagId, Guid TenantId) : IRequest<Result>;

public sealed class ClearOverrideCommandHandler : IRequestHandler<ClearOverrideCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public ClearOverrideCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(ClearOverrideCommand request, CancellationToken cancellationToken)
    {
        var existing = await _db.TenantFeatureFlags.IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.FeatureFlagId == request.FeatureFlagId && o.TenantId == request.TenantId && !o.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            _db.TenantFeatureFlags.Remove(existing);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}
