using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Domain.Errors;

/// <summary>
/// Centralized catalog of stable error codes returned by the domain layer.
/// </summary>
public static class DomainErrors
{
    public static class Tenant
    {
        public static readonly Error NotFound = Error.NotFound("tenant.not_found", "Tenant not found.");
        public static readonly Error Suspended = Error.Forbidden("tenant.suspended", "Tenant is suspended.");
        public static readonly Error SlugAlreadyExists = Error.Conflict("tenant.slug_exists", "Another tenant with the same slug already exists.");
        public static readonly Error CannotResolve = Error.Unauthorized("tenant.cannot_resolve", "Unable to resolve tenant from the request.");
    }

    public static class User
    {
        public static readonly Error NotFound = Error.NotFound("user.not_found", "User not found.");
        public static readonly Error EmailAlreadyExists = Error.Conflict("user.email_exists", "A user with the same email already exists.");
        public static readonly Error InvalidCredentials = Error.Unauthorized("user.invalid_credentials", "Invalid email or password.");
        public static readonly Error AccountLocked = Error.Forbidden("user.account_locked", "This account is locked. Please contact your administrator.");
        public static readonly Error AccountInactive = Error.Forbidden("user.account_inactive", "This account is inactive.");
    }

    public static class Authentication
    {
        public static readonly Error InvalidToken = Error.Unauthorized("auth.invalid_token", "The provided token is invalid or has expired.");
        public static readonly Error RefreshTokenExpired = Error.Unauthorized("auth.refresh_expired", "The refresh token has expired.");
        public static readonly Error RefreshTokenRevoked = Error.Unauthorized("auth.refresh_revoked", "The refresh token has been revoked.");
    }
}
