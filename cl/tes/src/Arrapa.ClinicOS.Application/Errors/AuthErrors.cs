using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

/// <summary>
/// Application-layer error catalog specific to authentication and profile flows.
/// </summary>
public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "auth.invalid_credentials", "Invalid email or password.");

    public static readonly Error TenantRequired = Error.Validation(
        "auth.tenant_required", "A tenant must be specified to sign in.");

    public static readonly Error TenantNotFound = Error.NotFound(
        "auth.tenant_not_found", "The specified tenant could not be found.");

    public static readonly Error TenantSuspended = Error.Forbidden(
        "auth.tenant_suspended", "This tenant is suspended. Please contact your administrator.");

    public static readonly Error AccountLocked = Error.Forbidden(
        "auth.account_locked", "This account is locked. Please try again later.");

    public static readonly Error AccountInactive = Error.Forbidden(
        "auth.account_inactive", "This account is inactive.");

    public static readonly Error EmailNotConfirmed = Error.Forbidden(
        "auth.email_not_confirmed", "Please confirm your email address before signing in.");

    public static readonly Error RefreshTokenInvalid = Error.Unauthorized(
        "auth.refresh_invalid", "The refresh token is invalid or has been revoked.");

    public static readonly Error RefreshTokenExpired = Error.Unauthorized(
        "auth.refresh_expired", "The refresh token has expired.");

    public static readonly Error CurrentPasswordIncorrect = Error.Validation(
        "auth.current_password_incorrect", "The current password is incorrect.");

    public static readonly Error PasswordResetFailed = Error.Failure(
        "auth.reset_failed", "Password reset could not be completed. The link may have expired.");

    public static readonly Error NotAuthenticated = Error.Unauthorized(
        "auth.not_authenticated", "You are not authenticated.");

    public static readonly Error UserNotFound = Error.NotFound(
        "auth.user_not_found", "User not found.");

    public static readonly Error EmailAlreadyRegistered = Error.Conflict(
        "auth.email_already_registered", "An account with this email already exists for the selected clinic.");

    public static readonly Error RegistrationFailed = Error.Failure(
        "auth.registration_failed", "Registration could not be completed. Please try again.");

    public static readonly Error EmailVerificationFailed = Error.Failure(
        "auth.email_verification_failed", "This email verification link is invalid or has expired.");

    public static readonly Error TwoFactorAlreadyEnabled = Error.Conflict(
        "auth.2fa.already_enabled", "Two-factor authentication is already enabled for this account.");

    public static readonly Error TwoFactorNotEnabled = Error.Failure(
        "auth.2fa.not_enabled", "Two-factor authentication is not enabled for this account.");

    public static readonly Error TwoFactorCodeInvalid = Error.Validation(
        "auth.2fa.code_invalid", "The verification code is incorrect.");
}
