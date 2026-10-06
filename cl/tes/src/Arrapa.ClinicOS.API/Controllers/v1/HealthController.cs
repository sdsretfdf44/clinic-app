using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1;

/// <summary>
/// Basic liveness endpoint. The full <c>/health</c> readiness check is registered separately via <see cref="Extensions.HealthCheckExtensions"/>.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[AllowAnonymous]
public sealed class HealthController : ApiControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(ApiResponse<object>.Ok(
        new { service = AppConstants.ApplicationName, status = "ok", utc = DateTimeOffset.UtcNow },
        "pong",
        CorrelationId));
}
