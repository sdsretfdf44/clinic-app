using Arrapa.ClinicOS.Application.Abstractions.Email;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Initiates a password reset. Always returns success to prevent account enumeration.
/// </summary>
public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantLookupService _tenantLookup;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITenantLookupService tenantLookup,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _tenantLookup = tenantLookup;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenantSlug))
        {
            _logger.LogInformation("Forgot password request without tenant slug for {Email}.", request.Email);
            return Result.Success();
        }

        var tenant = await _tenantLookup.FindBySlugAsync(request.TenantSlug, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            _logger.LogInformation("Forgot password request for unknown tenant slug {Slug}.", request.TenantSlug);
            return Result.Success();
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.NormalizedEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || !user.IsActive)
        {
            _logger.LogInformation("Forgot password request for unknown/inactive user {Email} in tenant {Slug}.", request.Email, tenant.Slug);
            return Result.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email ?? request.Email);

        var resetUrl = request.ResetUrlTemplate
            .Replace("{token}", encodedToken, StringComparison.OrdinalIgnoreCase)
            .Replace("{email}", encodedEmail, StringComparison.OrdinalIgnoreCase)
            .Replace("{tenant}", tenant.Slug, StringComparison.OrdinalIgnoreCase);

        var body = $"""
                    <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
                    <p>We received a request to reset your password for <strong>{System.Net.WebUtility.HtmlEncode(tenant.Name)}</strong>.</p>
                    <p><a href="{resetUrl}">Click here to reset your password</a>. This link expires in 24 hours.</p>
                    <p>If you didn't request this, you can safely ignore this email.</p>
                    """;

        await _emailService.SendAsync(new EmailMessage(
            To: user.Email ?? request.Email,
            Subject: $"Reset your {tenant.Name} password",
            HtmlBody: body,
            TextBody: $"Reset your password: {resetUrl}",
            ToDisplayName: user.FullName), cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
