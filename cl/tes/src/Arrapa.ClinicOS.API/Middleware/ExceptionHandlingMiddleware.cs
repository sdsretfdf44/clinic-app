using System.Net;
using System.Text.Json;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;

namespace Arrapa.ClinicOS.API.Middleware;

/// <summary>
/// Terminal exception handler that converts any unhandled exception into a consistent <see cref="ApiResponse{T}"/> payload
/// and an appropriate HTTP status code. Domain <see cref="ValidationException"/> is mapped to 400 Bad Request.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (ValidationException validationException)
        {
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, BuildValidationError(validationException)).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException unauthorized)
        {
            _logger.LogWarning(unauthorized, "Unauthorized access attempt.");
            await WriteResponseAsync(context, HttpStatusCode.Unauthorized, Error.Unauthorized("auth.unauthorized", "Unauthorized.")).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            var message = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.";
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError, Error.Unexpected("server.unexpected", message)).ConfigureAwait(false);
        }
    }

    private static Error BuildValidationError(ValidationException validationException)
    {
        var details = validationException.Errors
            .GroupBy(e => e.PropertyName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.OrdinalIgnoreCase);

        return Error.Validation("validation.failed", "One or more validation errors occurred.", details);
    }

    private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, Error error)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var correlationId = context.Items[AppConstants.Http.CorrelationIdHeader]?.ToString();

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var body = ApiResponse<object>.Fail(error, correlationId);
        var payload = JsonSerializer.Serialize(body, JsonOptions);
        await context.Response.WriteAsync(payload).ConfigureAwait(false);
    }
}
