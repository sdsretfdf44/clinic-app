using System.Diagnostics;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Application.Behaviors;

/// <summary>
/// Emits structured "request start / completed / failed" logs around every MediatR request, enriched with
/// tenant and user identifiers so log queries can slice by either dimension.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUser,
        ITenantContext tenantContext)
    {
        _logger = logger;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation(
            "Handling {Request}. User: {UserId}. Tenant: {TenantId}",
            requestName, _currentUser.UserId, _tenantContext.TenantId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next().ConfigureAwait(false);
            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {Request} in {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Request {Request} failed after {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
