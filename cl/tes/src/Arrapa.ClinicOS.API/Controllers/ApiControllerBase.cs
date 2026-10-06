using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers;

/// <summary>
/// Base controller providing the MediatR sender and consistent response-envelope helpers.
/// </summary>
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;

    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected string? CorrelationId => HttpContext.Items[AppConstants.Http.CorrelationIdHeader]?.ToString();

    protected IActionResult ToActionResult<T>(Result<T> result, string? successMessage = null) => result.IsSuccess
        ? Ok(ApiResponse<T>.Ok(result.Value, successMessage, CorrelationId))
        : StatusCode(MapStatusCode(result.Error), ApiResponse<T>.Fail(result.Error, CorrelationId));

    protected IActionResult ToActionResult(Result result, string? successMessage = null) => result.IsSuccess
        ? Ok(ApiResponse<object>.Ok(new { }, successMessage, CorrelationId))
        : StatusCode(MapStatusCode(result.Error), ApiResponse<object>.Fail(result.Error, CorrelationId));

    protected static int MapStatusCode(Error error) => error.Type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status400BadRequest,
        ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}
