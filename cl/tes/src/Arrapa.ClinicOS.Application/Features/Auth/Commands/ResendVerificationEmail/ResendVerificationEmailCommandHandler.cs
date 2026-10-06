using Arrapa.ClinicOS.Application.Abstractions.Email;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ResendVerificationEmail;

/// <summary>
/// Regenerates and re-sends the email verification link. Always returns success — the endpoint
/// deliberately doesn't disclose whether the account exists to avoid enumeration.
/// </summary>
public sealed class ResendVerificationEmailCommandHandler : IRequestHandler<ResendVerificationEmailCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantLookupService _tenantLookup;
    private readonly IEmailService _emailService;

    public ResendVerificationEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITenantLookupService tenantLookup,
        IEmailService emailService)
    {
        _userManager = userManager;
        _tenantLookup = tenantLookup;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantLookup.FindBySlugAsync(request.TenantSlug, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return Result.Success();
        }

        var normalized = request.Email.Trim().ToUpperInvariant();
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.NormalizedEmail == normalized, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || user.EmailConfirmed || !user.IsActive)
        {
            return Result.Success();
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
        var url = request.VerificationUrlTemplate
            .Replace("{token}", Uri.EscapeDataString(token), StringComparison.OrdinalIgnoreCase)
            .Replace("{email}", Uri.EscapeDataString(user.Email!), StringComparison.OrdinalIgnoreCase)
            .Replace("{tenant}", tenant.Slug, StringComparison.OrdinalIgnoreCase);

        await _emailService.SendAsync(new EmailMessage(
            To: user.Email!,
            Subject: $"Confirm your {tenant.Name} account",
            HtmlBody: $"""
                       <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
                       <p><a href="{url}">Click here to verify your email</a>. This link expires in 24 hours.</p>
                       """,
            TextBody: $"Confirm your email: {url}",
            ToDisplayName: user.FullName), cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
