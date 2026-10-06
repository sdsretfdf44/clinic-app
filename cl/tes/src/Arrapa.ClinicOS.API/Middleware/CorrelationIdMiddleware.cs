using Arrapa.ClinicOS.Shared.Constants;
using Serilog.Context;

namespace Arrapa.ClinicOS.API.Middleware;

/// <summary>
/// Attaches a correlation id to every request (accepts an incoming <c>X-Correlation-Id</c> header or generates one)
/// and pushes it onto the Serilog <see cref="LogContext"/> so all logs for the request share the same id.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(AppConstants.Http.CorrelationIdHeader, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : Guid.NewGuid().ToString("N");

        context.Response.Headers[AppConstants.Http.CorrelationIdHeader] = correlationId;
        context.Items[AppConstants.Http.CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context).ConfigureAwait(false);
        }
    }
}
