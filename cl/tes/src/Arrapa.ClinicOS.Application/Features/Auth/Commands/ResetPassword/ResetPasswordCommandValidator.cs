using FluentValidation;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.TenantSlug).NotEmpty().WithMessage("Tenant is required to reset a password.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10).WithMessage("Password must be at least 10 characters.")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must include an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must include a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must include a digit.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must include a symbol.");
    }
}
