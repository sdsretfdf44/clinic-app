namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Authenticated user password change. Requires knowledge of the existing password to prevent hijacked-session abuse.
/// </summary>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
