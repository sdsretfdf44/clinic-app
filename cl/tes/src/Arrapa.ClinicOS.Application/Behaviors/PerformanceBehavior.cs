using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Application.Behaviors;

/// <summary>
/// Logs a warning when any request exceeds the configured latency threshold — useful for spotting slow endpoints in production.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int WarnThresholdMs = 500;

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next().ConfigureAwait(false);
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > WarnThresholdMs)
        {
            _logger.LogWarning(
                "Long-running request: {Request} took {ElapsedMilliseconds}ms",
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
