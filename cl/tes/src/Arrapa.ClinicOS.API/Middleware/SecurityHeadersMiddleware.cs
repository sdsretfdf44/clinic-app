namespace Arrapa.ClinicOS.API.Middleware;

/// <summary>
/// Applies a strict set of security response headers on every response.
/// CSP is intentionally API-focused (no inline scripts, no framing) since this app serves JSON, not HTML;
/// the SPA has its own CSP served via nginx.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-site";
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        headers.Remove("X-Powered-By");
        headers.Remove("Server");

        return _next(context);
    }
}
