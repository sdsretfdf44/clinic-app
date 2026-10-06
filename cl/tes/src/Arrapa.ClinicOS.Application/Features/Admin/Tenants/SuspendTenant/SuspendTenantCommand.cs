using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.SuspendTenant;

public sealed record SuspendTenantCommand(Guid TenantId, string Reason) : IRequest<Result>;

public sealed class SuspendTenantCommandValidator : AbstractValidator<SuspendTenantCommand>
{
    public SuspendTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public SuspendTenantCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId == AppConstants.SystemTenants.PlatformTenantId)
        {
            return AdminErrors.Tenants.CannotSuspendPlatform;
        }

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
        {
            return AdminErrors.Tenants.NotFound;
        }

        if (tenant.Status == TenantStatus.Suspended)
        {
            return AdminErrors.Tenants.AlreadySuspended;
        }

        var now = _clock.UtcNow;
        tenant.Status = TenantStatus.Suspended;
        tenant.SuspendedAt = now;
        tenant.SuspensionReason = request.Reason.Trim();

        // Revoke all active refresh tokens for the tenant's users so open sessions cannot be renewed.
        var activeTokens = await _db.RefreshTokens.IgnoreQueryFilters()
            .Where(rt => rt.TenantId == tenant.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
