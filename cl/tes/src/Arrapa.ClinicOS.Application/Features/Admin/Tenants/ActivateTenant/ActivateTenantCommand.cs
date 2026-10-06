using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.ActivateTenant;

public sealed record ActivateTenantCommand(Guid TenantId) : IRequest<Result>;

public sealed class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public ActivateTenantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
        {
            return AdminErrors.Tenants.NotFound;
        }

        if (tenant.Status != TenantStatus.Suspended)
        {
            return AdminErrors.Tenants.NotSuspended;
        }

        tenant.Status = TenantStatus.Active;
        tenant.SuspendedAt = null;
        tenant.SuspensionReason = null;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
