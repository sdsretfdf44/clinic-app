namespace Arrapa.ClinicOS.Shared.Constants;

/// <summary>
/// Application-wide constants used across all layers.
/// </summary>
public static class AppConstants
{
    public const string ApplicationName = "Arrapa ClinicOS";
    public const string CompanyName = "Arrapa Software";

    public static class Http
    {
        public const string TenantHeader = "X-Tenant-Id";
        public const string CorrelationIdHeader = "X-Correlation-Id";
        public const string AuthorizationHeader = "Authorization";
        public const string BearerScheme = "Bearer";
    }

    public static class Claims
    {
        public const string TenantId = "tid";
        public const string UserId = "uid";
        public const string Email = "email";
        public const string FullName = "fullname";
        public const string Role = "role";
        public const string Permission = "permission";
    }

    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string ClinicAdmin = "ClinicAdmin";
        public const string Doctor = "Doctor";
        public const string Receptionist = "Receptionist";
        public const string Patient = "Patient";
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 200;
    }

    public static class Cache
    {
        public const string TenantByIdPrefix = "tenant:id:";
        public const string TenantBySlugPrefix = "tenant:slug:";
        public const int DefaultTtlSeconds = 300;
    }

    public static class SystemTenants
    {
        public static readonly Guid PlatformTenantId = new("00000000-0000-0000-0000-000000000001");
        public const string PlatformTenantSlug = "arrapa";
    }
}
