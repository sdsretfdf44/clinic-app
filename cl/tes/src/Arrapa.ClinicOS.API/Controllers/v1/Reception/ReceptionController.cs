using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Application.Features.Reception.GetReceptionQueue;
using Arrapa.ClinicOS.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Reception;

/// <summary>
/// Reception-desk endpoints. Everything reception needs today is a bucketed view over existing patient and
/// appointment surfaces — this controller is intentionally thin.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reception")]
[Authorize(Roles = AppConstants.Roles.Receptionist + "," + AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class ReceptionController : ApiControllerBase
{
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue(
        [FromQuery] Guid? doctorId = null,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetReceptionQueueQuery(doctorId, departmentId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }
}
