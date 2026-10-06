using FluentValidation;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(200);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10).WithMessage("Password must be at least 10 characters.")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must include an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must include a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must include a digit.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must include a symbol.")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password.");
    }
}
