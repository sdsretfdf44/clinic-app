using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.Enable2fa;

public sealed class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;

    public EnableTwoFactorCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
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

        if (await _userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false))
        {
            return AuthErrors.TwoFactorAlreadyEnabled;
        }

        var code = NormalizeCode(request.VerificationCode);
        var verified = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code).ConfigureAwait(false);
        if (!verified)
        {
            return AuthErrors.TwoFactorCodeInvalid;
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true).ConfigureAwait(false);
        return result.Succeeded ? Result.Success() : AuthErrors.TwoFactorCodeInvalid;
    }

    private static string NormalizeCode(string code) =>
        new(code.Where(char.IsDigit).ToArray());
}
