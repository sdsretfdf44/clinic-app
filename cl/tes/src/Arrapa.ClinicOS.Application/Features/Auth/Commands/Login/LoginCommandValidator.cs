using FluentValidation;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not a valid address.")
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(200);

        RuleFor(x => x.TenantSlug)
            .MaximumLength(64)
            .Matches("^[a-z0-9-]*$").When(x => !string.IsNullOrEmpty(x.TenantSlug))
            .WithMessage("Tenant slug may only contain lowercase letters, digits, and hyphens.");
    }
}
