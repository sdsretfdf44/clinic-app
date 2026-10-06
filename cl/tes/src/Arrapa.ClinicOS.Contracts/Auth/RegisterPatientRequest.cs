namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Public self-service patient registration payload. Creates the ApplicationUser (Patient role) and the
/// backing Patient record inside the given tenant, then emails a verification link.
/// </summary>
public sealed class RegisterPatientRequest
{
    public string TenantSlug { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
}
