using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Clinic.Profile;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Profile.GetClinicProfile;

public sealed record GetClinicProfileQuery : IRequest<Result<ClinicProfileResponse>>;

public sealed class GetClinicProfileQueryHandler : IRequestHandler<GetClinicProfileQuery, Result<ClinicProfileResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetClinicProfileQueryHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<ClinicProfileResponse>> Handle(GetClinicProfileQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<ClinicProfileResponse>.Failure(tenantIdResult.Error);
        }
        var tenantId = tenantIdResult.Value;

        var tenant = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
        {
            return ClinicErrors.NoTenantContext;
        }

        var planName = tenant.SubscriptionPlanId is null
            ? null
            : await _db.SubscriptionPlans.AsNoTracking().IgnoreQueryFilters()
                .Where(p => p.Id == tenant.SubscriptionPlanId).Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return new ClinicProfileResponse(
            tenant.Id, tenant.Name, tenant.Slug, tenant.Status.ToString(),
            tenant.ContactEmail, tenant.ContactPhone, tenant.LogoUrl,
            tenant.TimeZone, tenant.Locale, tenant.Currency,
            tenant.DefaultConsultationFee, planName, tenant.SubscriptionExpiresAt);
    }
}
