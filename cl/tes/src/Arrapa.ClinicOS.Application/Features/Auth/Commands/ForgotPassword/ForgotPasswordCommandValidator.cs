using FluentValidation;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.TenantSlug).MaximumLength(64);
        RuleFor(x => x.ResetUrlTemplate).NotEmpty();
    }
}
