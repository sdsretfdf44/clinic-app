using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

/// <summary>
/// Error catalog for super-admin operations.
/// </summary>
public static class AdminErrors
{
    public static class Tenants
    {
        public static readonly Error NotFound = Error.NotFound("admin.tenant.not_found", "Tenant not found.");
        public static readonly Error SlugAlreadyExists = Error.Conflict("admin.tenant.slug_exists", "Another tenant already uses this slug.");
        public static readonly Error CannotSuspendPlatform = Error.Forbidden("admin.tenant.cannot_suspend_platform", "The platform tenant cannot be suspended or deleted.");
        public static readonly Error AlreadySuspended = Error.Conflict("admin.tenant.already_suspended", "This tenant is already suspended.");
        public static readonly Error NotSuspended = Error.Conflict("admin.tenant.not_suspended", "This tenant is not currently suspended.");
        public static readonly Error AdminEmailInUse = Error.Conflict("admin.tenant.admin_email_in_use", "Another user in the platform already uses this email address.");
    }

    public static class Plans
    {
        public static readonly Error NotFound = Error.NotFound("admin.plan.not_found", "Subscription plan not found.");
        public static readonly Error CodeAlreadyExists = Error.Conflict("admin.plan.code_exists", "Another plan already uses this code.");
        public static readonly Error CannotDeactivateInUse = Error.Conflict("admin.plan.in_use", "This plan is currently assigned to one or more tenants and cannot be deactivated.");
    }

    public static class Users
    {
        public static readonly Error NotFound = Error.NotFound("admin.user.not_found", "User not found.");
        public static readonly Error CannotDeactivateSelf = Error.Forbidden("admin.user.cannot_deactivate_self", "You cannot deactivate your own account.");
    }

    public static class FeatureFlags
    {
        public static readonly Error NotFound = Error.NotFound("admin.feature_flag.not_found", "Feature flag not found.");
        public static readonly Error KeyAlreadyExists = Error.Conflict("admin.feature_flag.key_exists", "Another feature flag already uses this key.");
    }

    public static class Settings
    {
        public static readonly Error NotFound = Error.NotFound("admin.setting.not_found", "Setting not found.");
    }
}
