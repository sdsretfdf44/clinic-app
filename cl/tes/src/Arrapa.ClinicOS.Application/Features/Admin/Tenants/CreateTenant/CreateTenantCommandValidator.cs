using FluentValidation;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.CreateTenant;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug may only contain lowercase letters, digits, and hyphens.");
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(30);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Locale).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Currency).NotEmpty().Length(3, 10);
        RuleFor(x => x.AdminFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdminLastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
