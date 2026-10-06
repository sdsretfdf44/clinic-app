using System.Security.Claims;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace Arrapa.ClinicOS.Infrastructure.Identity;

/// <summary>
/// HTTP-context-backed implementation of <see cref="ICurrentUserService"/>.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var raw = User?.FindFirstValue(AppConstants.Claims.UserId) ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(AppConstants.Claims.Email) ?? User?.FindFirstValue(ClaimTypes.Email);
    public string? FullName => User?.FindFirstValue(AppConstants.Claims.FullName) ?? User?.Identity?.Name;

    public Guid? TenantId
    {
        get
        {
            var raw = User?.FindFirstValue(AppConstants.Claims.TenantId);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
