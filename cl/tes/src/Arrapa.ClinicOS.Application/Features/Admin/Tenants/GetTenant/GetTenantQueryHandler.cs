using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.GetTenant;

public sealed class GetTenantQueryHandler : IRequestHandler<GetTenantQuery, Result<TenantDetailResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetTenantQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<TenantDetailResponse>> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
        {
            return AdminErrors.Tenants.NotFound;
        }

        var planName = tenant.SubscriptionPlanId is null
            ? null
            : await _db.SubscriptionPlans.IgnoreQueryFilters()
                .Where(p => p.Id == tenant.SubscriptionPlanId).Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var userCount = await _db.Users.IgnoreQueryFilters()
            .CountAsync(u => u.TenantId == tenant.Id && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var response = new TenantDetailResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Status.ToString(),
            tenant.ContactEmail,
            tenant.ContactPhone,
            tenant.LogoUrl,
            tenant.TimeZone,
            tenant.Locale,
            tenant.Currency,
            tenant.SubscriptionPlanId,
            planName,
            tenant.SubscriptionExpiresAt,
            tenant.SuspendedAt,
            tenant.SuspensionReason,
            userCount,
            tenant.CreatedOn,
            tenant.UpdatedOn);

        return response;
    }
}
