using FluentValidation;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Refresh token is required.").MaximumLength(256);
    }
}
