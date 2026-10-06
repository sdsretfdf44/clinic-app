using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.DeleteTenant;

public sealed record DeleteTenantCommand(Guid TenantId) : IRequest<Result>;

public sealed class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteTenantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId == AppConstants.SystemTenants.PlatformTenantId)
        {
            return AdminErrors.Tenants.CannotSuspendPlatform;
        }

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null || tenant.IsDeleted)
        {
            return AdminErrors.Tenants.NotFound;
        }

        // Soft delete is applied by the audit interceptor.
        _db.Tenants.Remove(tenant);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
