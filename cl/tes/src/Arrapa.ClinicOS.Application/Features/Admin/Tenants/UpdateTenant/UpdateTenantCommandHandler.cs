using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.UpdateTenant;

public sealed class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<TenantDetailResponse>>
{
    private readonly IApplicationDbContext _db;

    public UpdateTenantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<TenantDetailResponse>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
        {
            return AdminErrors.Tenants.NotFound;
        }

        tenant.Name = request.Name.Trim();
        tenant.ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim();
        tenant.ContactPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? null : request.ContactPhone.Trim();
        tenant.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
        tenant.TimeZone = request.TimeZone.Trim();
        tenant.Locale = request.Locale.Trim();
        tenant.Currency = request.Currency.Trim().ToUpperInvariant();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var planName = tenant.SubscriptionPlanId is null
            ? null
            : await _db.SubscriptionPlans.IgnoreQueryFilters()
                .Where(p => p.Id == tenant.SubscriptionPlanId).Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var userCount = await _db.Users.IgnoreQueryFilters()
            .CountAsync(u => u.TenantId == tenant.Id && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        return new TenantDetailResponse(
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
    }
}
