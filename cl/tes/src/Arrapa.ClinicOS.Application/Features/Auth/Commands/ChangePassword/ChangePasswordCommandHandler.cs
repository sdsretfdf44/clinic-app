using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
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

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword).ConfigureAwait(false);
        if (!passwordValid)
        {
            return AuthErrors.CurrentPasswordIncorrect;
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var details = result.Errors
                .GroupBy(_ => "newPassword", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray(), StringComparer.OrdinalIgnoreCase);
            return Error.Validation("auth.change_password_failed", "Password could not be changed.", details);
        }

        return Result.Success();
    }
}
