using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.Login;
using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<AuthUserResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantLookupService _tenantLookup;

    public UpdateProfileCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        ITenantLookupService tenantLookup)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _tenantLookup = tenantLookup;
    }

    public async Task<Result<AuthUserResponse>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
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

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();

        var updateResult = await _userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!updateResult.Succeeded)
        {
            var details = updateResult.Errors
                .GroupBy(_ => "profile", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray(), StringComparer.OrdinalIgnoreCase);
            return Error.Validation("auth.profile_update_failed", "Profile could not be updated.", details);
        }

        var tenant = await _tenantLookup.FindByIdAsync(user.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return AuthErrors.TenantNotFound;
        }

        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        return LoginCommandHandler.BuildUserResponse(user, tenant, roles);
    }
}
