using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.UpdateTenant;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    string? LogoUrl,
    string TimeZone,
    string Locale,
    string Currency) : IRequest<Result<TenantDetailResponse>>;

public sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(30);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Locale).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Currency).NotEmpty().Length(3, 10);
    }
}
