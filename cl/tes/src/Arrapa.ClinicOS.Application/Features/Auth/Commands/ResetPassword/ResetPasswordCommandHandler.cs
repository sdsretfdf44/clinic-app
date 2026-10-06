using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantLookupService _tenantLookup;
    private readonly IApplicationDbContext _db;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITenantLookupService tenantLookup,
        IApplicationDbContext db)
    {
        _userManager = userManager;
        _tenantLookup = tenantLookup;
        _db = db;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantLookup.FindBySlugAsync(request.TenantSlug!, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return AuthErrors.TenantNotFound;
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.NormalizedEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return AuthErrors.PasswordResetFailed;
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var details = result.Errors
                .GroupBy(e => "password", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray(), StringComparer.OrdinalIgnoreCase);
            return Error.Validation(AuthErrors.PasswordResetFailed.Code, AuthErrors.PasswordResetFailed.Message, details);
        }

        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
        }
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
