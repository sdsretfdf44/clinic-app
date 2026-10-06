using FluentValidation;
using MediatR;

namespace Arrapa.ClinicOS.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators against every request. Aggregated failures
/// are raised as a <see cref="ValidationException"/> and translated into HTTP 400 by the exception middleware.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}
