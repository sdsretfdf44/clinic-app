using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.Disable2fa;

public sealed class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;

    public DisableTwoFactorCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return AuthErrors.NotAuthenticated;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);
        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false))
        {
            return AuthErrors.TwoFactorNotEnabled;
        }

        var code = new string(request.VerificationCode.Where(char.IsDigit).ToArray());
        var verified = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code).ConfigureAwait(false);
        if (!verified)
        {
            return AuthErrors.TwoFactorCodeInvalid;
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false).ConfigureAwait(false);
        await _userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(false);
        return Result.Success();
    }
}
