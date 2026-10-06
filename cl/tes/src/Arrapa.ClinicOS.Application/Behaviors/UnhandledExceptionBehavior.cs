using MediatR;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Application.Behaviors;

/// <summary>
/// Catches unhandled exceptions from downstream handlers, logs them with full request context, and rethrows so the
/// global exception middleware can shape the HTTP response.
/// </summary>
public sealed class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {Request}: {Message}", typeof(TRequest).Name, ex.Message);
            throw;
        }
    }
}
