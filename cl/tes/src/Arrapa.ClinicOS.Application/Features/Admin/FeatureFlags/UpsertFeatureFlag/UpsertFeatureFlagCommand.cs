using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Admin.FeatureFlags;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.FeatureFlags.UpsertFeatureFlag;

/// <summary>
/// Creates a new feature flag when <see cref="Id"/> is null; otherwise updates the existing flag.
/// </summary>
public sealed record UpsertFeatureFlagCommand(
    Guid? Id,
    string Key,
    string Name,
    string? Description,
    bool DefaultEnabled,
    bool IsGlobal) : IRequest<Result<FeatureFlagResponse>>;

public sealed class UpsertFeatureFlagCommandValidator : AbstractValidator<UpsertFeatureFlagCommand>
{
    public UpsertFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(128).Matches("^[a-z0-9_]+$").WithMessage("Key may only contain lowercase letters, digits, and underscores.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpsertFeatureFlagCommandHandler : IRequestHandler<UpsertFeatureFlagCommand, Result<FeatureFlagResponse>>
{
    private readonly IApplicationDbContext _db;

    public UpsertFeatureFlagCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<FeatureFlagResponse>> Handle(UpsertFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim().ToLowerInvariant();
        var duplicate = await _db.FeatureFlags.IgnoreQueryFilters()
            .AnyAsync(f => f.Key == key && (request.Id == null || f.Id != request.Id), cancellationToken).ConfigureAwait(false);
        if (duplicate)
        {
            return AdminErrors.FeatureFlags.KeyAlreadyExists;
        }

        Domain.FeatureFlags.FeatureFlag flag;
        if (request.Id.HasValue)
        {
            var existing = await _db.FeatureFlags.IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == request.Id.Value && !f.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (existing is null)
            {
                return AdminErrors.FeatureFlags.NotFound;
            }
            flag = existing;
        }
        else
        {
            flag = new Domain.FeatureFlags.FeatureFlag
            {
                Id = Guid.NewGuid(),
                TenantId = AppConstants.SystemTenants.PlatformTenantId
            };
            await _db.FeatureFlags.AddAsync(flag, cancellationToken).ConfigureAwait(false);
        }

        flag.Key = key;
        flag.Name = request.Name.Trim();
        flag.Description = request.Description?.Trim();
        flag.DefaultEnabled = request.DefaultEnabled;
        flag.IsGlobal = request.IsGlobal;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var overrideCount = await _db.TenantFeatureFlags.IgnoreQueryFilters()
            .CountAsync(o => o.FeatureFlagId == flag.Id && !o.IsDeleted, cancellationToken).ConfigureAwait(false);

        return new FeatureFlagResponse(flag.Id, flag.Key, flag.Name, flag.Description, flag.DefaultEnabled, flag.IsGlobal, overrideCount);
    }
}
