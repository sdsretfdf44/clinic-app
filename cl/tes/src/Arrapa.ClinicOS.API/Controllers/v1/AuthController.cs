using Arrapa.ClinicOS.API.Extensions;
using Arrapa.ClinicOS.API.Infrastructure;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.ChangePassword;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.Disable2fa;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.Enable2fa;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.ForgotPassword;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.Login;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.Logout;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.RefreshToken;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.RegisterPatient;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.ResendVerificationEmail;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.ResetPassword;
using Arrapa.ClinicOS.Application.Features.Auth.Commands.VerifyEmail;
using Arrapa.ClinicOS.Application.Features.Auth.Queries.Get2faStatus;
using Arrapa.ClinicOS.Application.Features.Auth.Queries.GetCurrentUser;
using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Infrastructure.Email;
using Arrapa.ClinicOS.Shared.Results;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Arrapa.ClinicOS.API.Controllers.v1;

/// <summary>
/// Authentication endpoints: sign in / refresh / sign out / password lifecycle / current user /
/// public patient registration + email verification / TOTP two-factor authentication.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
public sealed class AuthController : ApiControllerBase
{
    private readonly IOptions<EmailOptions> _emailOptions;

    public AuthController(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.TenantSlug,
            HttpContext.GetClientIpAddress(),
            HttpContext.Request.Headers.UserAgent.ToString(),
            request.TwoFactorCode);

        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Signed in successfully.");
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthTokensResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(
            request.RefreshToken,
            HttpContext.GetClientIpAddress(),
            HttpContext.Request.Headers.UserAgent.ToString());

        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Signed out.");
    }

    [HttpPost("register-patient")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterPatientCommand(
            request.TenantSlug,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Password,
            request.DateOfBirth,
            BuildVerificationUrlTemplate());

        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Registration successful. Please check your email to verify your account.");
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(request.TenantSlug, request.Email, request.Token);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Your email has been verified.");
    }

    [HttpPost("resend-verification-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request, CancellationToken cancellationToken)
    {
        var command = new ResendVerificationEmailCommand(request.TenantSlug, request.Email, BuildVerificationUrlTemplate());
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "If an account exists for that email, a verification link has been sent.");
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var baseUrl = _emailOptions.Value.WebBaseUrl?.TrimEnd('/') ?? string.Empty;
        var template = _emailOptions.Value.PasswordResetUrlTemplate;
        var resetUrlTemplate = template.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? template
            : $"{baseUrl}{template}";

        var command = new ForgotPasswordCommand(request.Email, request.TenantSlug, resetUrlTemplate);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "If an account exists for that email, a reset link has been sent.");
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword, request.TenantSlug);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Your password has been reset. Please sign in.");
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Your password has been changed.");
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCurrentUserQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("2fa")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetTwoFactorSetupResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTwoFactor(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetTwoFactorSetupQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new EnableTwoFactorCommand(request.VerificationCode), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Two-factor authentication is now enabled.");
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DisableTwoFactorCommand(request.VerificationCode), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Two-factor authentication is now disabled.");
    }

    private string BuildVerificationUrlTemplate()
    {
        var baseUrl = _emailOptions.Value.WebBaseUrl?.TrimEnd('/') ?? string.Empty;
        var template = _emailOptions.Value.EmailVerificationUrlTemplate;
        return template.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? template : $"{baseUrl}{template}";
    }
}
