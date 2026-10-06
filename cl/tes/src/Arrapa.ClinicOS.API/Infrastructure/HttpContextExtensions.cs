using Microsoft.AspNetCore.HttpOverrides;

namespace Arrapa.ClinicOS.API.Infrastructure;

/// <summary>
/// Small helpers for accessing HTTP request metadata (client IP, correlation id, etc.).
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Returns the caller's IP address, preferring <c>X-Forwarded-For</c> when the app is behind a proxy that has been
    /// whitelisted via <see cref="ForwardedHeadersOptions"/>.
    /// </summary>
    public static string? GetClientIpAddress(this HttpContext context)
    {
        var remote = context.Connection.RemoteIpAddress;
        return remote?.ToString();
    }
}
