namespace Arrapa.ClinicOS.Contracts.Auth;

/// <summary>
/// Updates the current user's personal profile. All fields are required.
/// </summary>
public sealed record UpdateProfileRequest(string FirstName, string LastName, string? PhoneNumber, string? AvatarUrl);
