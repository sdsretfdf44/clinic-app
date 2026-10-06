using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Arrapa.ClinicOS.API.Extensions;

/// <summary>
/// Configures ASP.NET Core rate limiting policies. Auth endpoints get a strict per-client policy
/// to blunt brute-force attempts; the rest of the API gets a generous global fixed window.
/// </summary>
public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";
    public const string GlobalPolicy = "global";

    public static IServiceCollection AddArrapaRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RateLimiting");
        var authPermitPerMinute = section.GetValue<int?>("AuthPermitPerMinute") ?? 10;
        var globalPermitPerMinute = section.GetValue<int?>("GlobalPermitPerMinute") ?? 300;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                await context.HttpContext.Response.WriteAsync(
                    "{\"success\":false,\"error\":{\"code\":\"rate.limit.exceeded\",\"message\":\"Too many requests. Please slow down.\"}}",
                    cancellationToken).ConfigureAwait(false);
            };

            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolveClientKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = authPermitPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy(GlobalPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolveClientKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = globalPermitPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        return services;
    }

    private static string ResolveClientKey(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"user:{userId}";
        }

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ip = !string.IsNullOrWhiteSpace(forwardedFor)
            ? forwardedFor.Split(',')[0].Trim()
            : httpContext.Connection.RemoteIpAddress?.ToString();

        return $"ip:{ip ?? "unknown"}";
    }
}
