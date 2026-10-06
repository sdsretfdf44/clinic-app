namespace Arrapa.ClinicOS.Application.Abstractions.Identity;

/// <summary>
/// Resolves information about the authenticated user for the current request.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    Guid? TenantId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
