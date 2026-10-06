using System.Security.Claims;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Infrastructure.MultiTenancy;

/// <summary>
/// Resolves the current tenant in the following priority order:
/// <list type="number">
///   <item>The <c>tid</c> claim of the authenticated principal.</item>
///   <item>The <c>X-Tenant-Id</c> HTTP header (accepts either a GUID or a slug).</item>
///   <item>The left-most DNS label of the host (e.g., <c>acme.clinicos.arrapa.com</c> → slug <c>acme</c>).</item>
/// </list>
/// <para>
/// Because this middleware runs after <see cref="Microsoft.AspNetCore.Builder.AuthAppBuilderExtensions.UseAuthentication"/>
/// but before <see cref="Microsoft.AspNetCore.Builder.AuthorizationAppBuilderExtensions.UseAuthorization"/>,
/// <see cref="HttpContext.User"/> may not yet reflect the JWT scheme's principal. To pick up the
/// <c>tid</c> claim reliably, we proactively call <see cref="IAuthenticationService.AuthenticateAsync"/>
/// with the JWT bearer scheme when an <c>Authorization: Bearer</c> header is present.
/// </para>
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private static readonly HashSet<string> IgnoredHostLabels =
        new(StringComparer.OrdinalIgnoreCase) { "www", "api", "app", "localhost", "clinicos", "arrapa" };

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ILogger<TenantResolutionMiddleware> logger)
    {
        var principal = await ResolveAuthenticatedPrincipalAsync(context).ConfigureAwait(false);

        var fromClaim = TryResolveFromClaim(principal, tenantContext);
        var fromHeader = !fromClaim && TryResolveFromHeader(context, tenantContext);
        var fromSubdomain = !fromClaim && !fromHeader && TryResolveFromSubdomain(context, tenantContext);

        logger.LogInformation(
            "Tenant resolution: user={IsAuthed}, claim={FromClaim}, header={FromHeader}, subdomain={FromSubdomain}, tid={TenantId}",
            principal?.Identity?.IsAuthenticated ?? false, fromClaim, fromHeader, fromSubdomain, tenantContext.TenantId);

        await _next(context).ConfigureAwait(false);
    }

    private static async Task<ClaimsPrincipal?> ResolveAuthenticatedPrincipalAsync(HttpContext context)
    {
        // If the pipeline already established an authenticated principal, use it directly.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            return context.User;
        }

        // No principal yet — attempt an explicit JWT authenticate when a bearer token is present. This avoids
        // duplicating token validation logic and keeps the tenant claim consistent with what authorization sees
        // later in the pipeline.
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var result = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme).ConfigureAwait(false);
        if (result.Succeeded && result.Principal is not null)
        {
            context.User = result.Principal;
            return result.Principal;
        }

        return null;
    }

    private static bool TryResolveFromClaim(ClaimsPrincipal? user, ITenantContext tenantContext)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var raw = user.FindFirstValue(AppConstants.Claims.TenantId);
        if (Guid.TryParse(raw, out var id) && id != Guid.Empty)
        {
            tenantContext.SetTenant(id);
            return true;
        }

        return false;
    }

    private static bool TryResolveFromHeader(HttpContext context, ITenantContext tenantContext)
    {
        if (!context.Request.Headers.TryGetValue(AppConstants.Http.TenantHeader, out var values))
        {
            return false;
        }

        var value = values.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (Guid.TryParse(value, out var id) && id != Guid.Empty)
        {
            tenantContext.SetTenant(id);
            return true;
        }

        // Slug provided; final resolution to Guid happens in a dedicated lookup service (added in a later phase).
        tenantContext.SetTenant(Guid.Empty, value.Trim().ToLowerInvariant());
        return true;
    }

    private static bool TryResolveFromSubdomain(HttpContext context, ITenantContext tenantContext)
    {
        var host = context.Request.Host.Host;
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        var labels = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (labels.Length < 3)
        {
            return false;
        }

        var candidate = labels[0];
        if (IgnoredHostLabels.Contains(candidate))
        {
            return false;
        }

        tenantContext.SetTenant(Guid.Empty, candidate.ToLowerInvariant());
        return true;
    }
}
