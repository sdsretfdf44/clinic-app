using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.FeatureFlags;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.FeatureFlags.SetOverride;

public sealed record SetOverrideCommand(Guid FeatureFlagId, Guid TenantId, bool IsEnabled) : IRequest<Result>;

public sealed class SetOverrideCommandValidator : AbstractValidator<SetOverrideCommand>
{
    public SetOverrideCommandValidator()
    {
        RuleFor(x => x.FeatureFlagId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public sealed class SetOverrideCommandHandler : IRequestHandler<SetOverrideCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetOverrideCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(SetOverrideCommand request, CancellationToken cancellationToken)
    {
        var flag = await _db.FeatureFlags.IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == request.FeatureFlagId && !f.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (flag is null)
        {
            return AdminErrors.FeatureFlags.NotFound;
        }

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return AdminErrors.Tenants.NotFound;
        }

        var existing = await _db.TenantFeatureFlags.IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.FeatureFlagId == flag.Id && o.TenantId == request.TenantId && !o.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            existing = new TenantFeatureFlag
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                FeatureFlagId = flag.Id,
                IsEnabled = request.IsEnabled
            };
            await _db.TenantFeatureFlags.AddAsync(existing, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            existing.IsEnabled = request.IsEnabled;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
