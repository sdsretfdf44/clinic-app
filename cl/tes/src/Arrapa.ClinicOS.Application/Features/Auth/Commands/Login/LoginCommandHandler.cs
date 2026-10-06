using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Domain.Tenants;
using RefreshTokenEntity = Arrapa.ClinicOS.Domain.Identity.RefreshToken;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthTokensResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantLookupService _tenantLookup;
    private readonly ITenantContext _tenantContext;
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenService _tokenService;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITenantLookupService tenantLookup,
        ITenantContext tenantContext,
        IApplicationDbContext db,
        IJwtTokenService tokenService,
        IDateTimeProvider clock,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _tenantLookup = tenantLookup;
        _tenantContext = tenantContext;
        _db = db;
        _tokenService = tokenService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<AuthTokensResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var tenantResult = await ResolveTenantAsync(request.TenantSlug, cancellationToken).ConfigureAwait(false);
        if (tenantResult.IsFailure)
        {
            return Result<AuthTokensResponse>.Failure(tenantResult.Error);
        }

        var tenant = tenantResult.Value;
        if (tenant.Status is TenantStatus.Suspended or TenantStatus.Cancelled or TenantStatus.Expired)
        {
            return AuthErrors.TenantSuspended;
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.NormalizedEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            _logger.LogInformation("Login attempt for unknown user {Email} in tenant {TenantSlug}.", request.Email, tenant.Slug);
            return AuthErrors.InvalidCredentials;
        }

        if (!user.IsActive)
        {
            return AuthErrors.AccountInactive;
        }

        if (await _userManager.IsLockedOutAsync(user).ConfigureAwait(false))
        {
            return AuthErrors.AccountLocked;
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user).ConfigureAwait(false);
            _logger.LogWarning("Failed login for {Email} in tenant {TenantSlug}.", request.Email, tenant.Slug);
            return AuthErrors.InvalidCredentials;
        }

        if (await _userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false))
        {
            if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
            {
                return Error.Validation("auth.2fa.required", "Two-factor authentication code required.");
            }

            var digitsOnly = new string(request.TwoFactorCode.Where(char.IsDigit).ToArray());
            var twoFactorValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, digitsOnly).ConfigureAwait(false);
            if (!twoFactorValid)
            {
                await _userManager.AccessFailedAsync(user).ConfigureAwait(false);
                return AuthErrors.TwoFactorCodeInvalid;
            }
        }

        await _userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);

        _tenantContext.SetTenant(tenant.Id, tenant.Slug);

        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

        var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles, cancellationToken).ConfigureAwait(false);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshEntity = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserId = user.Id,
            Token = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent
        };
        await _db.RefreshTokens.AddAsync(refreshEntity, cancellationToken).ConfigureAwait(false);

        user.LastLoginAt = _clock.UtcNow;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = new AuthTokensResponse(
            accessToken.Token,
            accessToken.TokenType,
            accessToken.ExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            BuildUserResponse(user, tenant, roles));

        return response;
    }

    private async Task<Result<Tenant>> ResolveTenantAsync(string? tenantSlug, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(tenantSlug))
        {
            var bySlug = await _tenantLookup.FindBySlugAsync(tenantSlug, cancellationToken).ConfigureAwait(false);
            return bySlug is null ? AuthErrors.TenantNotFound : Result<Tenant>.Success(bySlug);
        }

        if (_tenantContext.TenantId is Guid tenantId && tenantId != Guid.Empty)
        {
            var byId = await _tenantLookup.FindByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
            return byId is null ? AuthErrors.TenantNotFound : Result<Tenant>.Success(byId);
        }

        if (!string.IsNullOrWhiteSpace(_tenantContext.Slug))
        {
            var bySlug = await _tenantLookup.FindBySlugAsync(_tenantContext.Slug, cancellationToken).ConfigureAwait(false);
            return bySlug is null ? AuthErrors.TenantNotFound : Result<Tenant>.Success(bySlug);
        }

        return AuthErrors.TenantRequired;
    }

    internal static AuthUserResponse BuildUserResponse(ApplicationUser user, Tenant tenant, IEnumerable<string> roles) => new(
        user.Id,
        user.Email ?? string.Empty,
        user.FirstName,
        user.LastName,
        user.FullName,
        user.AvatarUrl,
        user.PhoneNumber,
        tenant.Id,
        tenant.Slug,
        tenant.Name,
        user.PrimaryRole.ToString(),
        roles.ToArray(),
        user.LastLoginAt);
}
