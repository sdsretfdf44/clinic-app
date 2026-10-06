using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Clinic.Profile;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Profile.UpdateClinicProfile;

public sealed record UpdateClinicProfileCommand(
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? LogoUrl,
    string TimeZone,
    string Locale,
    string Currency,
    decimal? DefaultConsultationFee) : IRequest<Result<ClinicProfileResponse>>;

public sealed class UpdateClinicProfileCommandValidator : AbstractValidator<UpdateClinicProfileCommand>
{
    public UpdateClinicProfileCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(30);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Locale).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Currency).NotEmpty().Length(3, 10);
        RuleFor(x => x.DefaultConsultationFee).GreaterThanOrEqualTo(0m).When(x => x.DefaultConsultationFee.HasValue);
    }
}

public sealed class UpdateClinicProfileCommandHandler : IRequestHandler<UpdateClinicProfileCommand, Result<ClinicProfileResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public UpdateClinicProfileCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<ClinicProfileResponse>> Handle(UpdateClinicProfileCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<ClinicProfileResponse>.Failure(tenantIdResult.Error);
        }

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantIdResult.Value && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
        {
            return ClinicErrors.NoTenantContext;
        }

        tenant.Name = request.Name.Trim();
        tenant.ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim();
        tenant.ContactPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? null : request.ContactPhone.Trim();
        tenant.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
        tenant.TimeZone = request.TimeZone.Trim();
        tenant.Locale = request.Locale.Trim();
        tenant.Currency = request.Currency.Trim().ToUpperInvariant();
        tenant.DefaultConsultationFee = request.DefaultConsultationFee;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var planName = tenant.SubscriptionPlanId is null
            ? null
            : await _db.SubscriptionPlans.IgnoreQueryFilters()
                .Where(p => p.Id == tenant.SubscriptionPlanId).Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return new ClinicProfileResponse(
            tenant.Id, tenant.Name, tenant.Slug, tenant.Status.ToString(),
            tenant.ContactEmail, tenant.ContactPhone, tenant.LogoUrl,
            tenant.TimeZone, tenant.Locale, tenant.Currency,
            tenant.DefaultConsultationFee, planName, tenant.SubscriptionExpiresAt);
    }
}
