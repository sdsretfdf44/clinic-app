using System.Text;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Arrapa.ClinicOS.Application.Features.Auth.Queries.Get2faStatus;

public sealed class GetTwoFactorSetupQueryHandler : IRequestHandler<GetTwoFactorSetupQuery, Result<GetTwoFactorSetupResponse>>
{
    private const string Issuer = "Arrapa ClinicOS";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;

    public GetTwoFactorSetupQueryHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<Result<GetTwoFactorSetupResponse>> Handle(GetTwoFactorSetupQuery request, CancellationToken cancellationToken)
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

        var enabled = await _userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(false);
        if (enabled)
        {
            return new GetTwoFactorSetupResponse(new TwoFactorStatusResponse(true), Setup: null);
        }

        var sharedKey = await _userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(sharedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(false);
            sharedKey = await _userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(false);
        }

        var email = user.Email ?? user.UserName ?? userId.ToString();
        var otpauthUri = BuildAuthenticatorUri(email, sharedKey!);

        return new GetTwoFactorSetupResponse(
            new TwoFactorStatusResponse(false),
            new TwoFactorSetupResponse(FormatSharedKey(sharedKey!), otpauthUri));
    }

    private static string BuildAuthenticatorUri(string email, string sharedKey)
    {
        var encodedIssuer = Uri.EscapeDataString(Issuer);
        var encodedLabel = Uri.EscapeDataString($"{Issuer}:{email}");
        return $"otpauth://totp/{encodedLabel}?secret={sharedKey}&issuer={encodedIssuer}&digits=6";
    }

    private static string FormatSharedKey(string key)
    {
        var buffer = new StringBuilder();
        for (var i = 0; i < key.Length; i += 4)
        {
            buffer.Append(key.AsSpan(i, Math.Min(4, key.Length - i)));
            if (i + 4 < key.Length)
            {
                buffer.Append(' ');
            }
        }
        return buffer.ToString().ToLowerInvariant();
    }
}
