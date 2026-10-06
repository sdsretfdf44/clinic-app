using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantLookupService _tenantLookup;

    public VerifyEmailCommandHandler(UserManager<ApplicationUser> userManager, ITenantLookupService tenantLookup)
    {
        _userManager = userManager;
        _tenantLookup = tenantLookup;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantLookup.FindBySlugAsync(request.TenantSlug, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return AuthErrors.EmailVerificationFailed;
        }

        var normalized = request.Email.Trim().ToUpperInvariant();
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.NormalizedEmail == normalized, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return AuthErrors.EmailVerificationFailed;
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        var result = await _userManager.ConfirmEmailAsync(user, request.Token).ConfigureAwait(false);
        return result.Succeeded ? Result.Success() : AuthErrors.EmailVerificationFailed;
    }
}
