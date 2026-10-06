using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.Login;
using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Arrapa.ClinicOS.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<AuthUserResponse>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantLookupService _tenantLookup;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUser,
        UserManager<ApplicationUser> userManager,
        ITenantLookupService tenantLookup)
    {
        _currentUser = currentUser;
        _userManager = userManager;
        _tenantLookup = tenantLookup;
    }

    public async Task<Result<AuthUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
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

        var tenant = await _tenantLookup.FindByIdAsync(user.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return AuthErrors.TenantNotFound;
        }

        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        return LoginCommandHandler.BuildUserResponse(user, tenant, roles);
    }
}
