using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.Login;
using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokensResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantLookupService _tenantLookup;
    private readonly ITenantContext _tenantContext;
    private readonly IJwtTokenService _tokenService;
    private readonly IDateTimeProvider _clock;

    public RefreshTokenCommandHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ITenantLookupService tenantLookup,
        ITenantContext tenantContext,
        IJwtTokenService tokenService,
        IDateTimeProvider clock)
    {
        _db = db;
        _userManager = userManager;
        _tenantLookup = tenantLookup;
        _tenantContext = tenantContext;
        _tokenService = tokenService;
        _clock = clock;
    }

    public async Task<Result<AuthTokensResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken)
            .ConfigureAwait(false);

        if (stored is null || stored.RevokedAt is not null)
        {
            return AuthErrors.RefreshTokenInvalid;
        }

        if (_clock.UtcNow >= stored.ExpiresAt)
        {
            return AuthErrors.RefreshTokenExpired;
        }

        var user = stored.User;
        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return AuthErrors.AccountInactive;
        }

        var tenant = await _tenantLookup.FindByIdAsync(user.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return AuthErrors.TenantNotFound;
        }

        _tenantContext.SetTenant(tenant.Id, tenant.Slug);

        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles, cancellationToken).ConfigureAwait(false);
        var newRefresh = _tokenService.GenerateRefreshToken();

        stored.RevokedAt = _clock.UtcNow;
        stored.ReplacedByToken = newRefresh.Token[..Math.Min(64, newRefresh.Token.Length)];

        var replacement = new Domain.Identity.RefreshToken
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = user.Id,
            Token = newRefresh.Token,
            ExpiresAt = newRefresh.ExpiresAt,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent
        };
        await _db.RefreshTokens.AddAsync(replacement, cancellationToken).ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = new AuthTokensResponse(
            accessToken.Token,
            accessToken.TokenType,
            accessToken.ExpiresAt,
            newRefresh.Token,
            newRefresh.ExpiresAt,
            LoginCommandHandler.BuildUserResponse(user, tenant, roles));

        return response;
    }
}
