using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Application.Features.Notifications;
using Arrapa.ClinicOS.Contracts.Notifications;
using Arrapa.ClinicOS.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Notifications;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize(Roles =
    AppConstants.Roles.ClinicAdmin + "," +
    AppConstants.Roles.Receptionist + "," +
    AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class NotificationsController : ApiControllerBase
{
    [HttpGet("logs")]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? appointmentId = null,
        [FromQuery] string? kind = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListNotificationLogsQuery(pageNumber, pageSize, appointmentId, kind, channel, status, search);
        var result = await Sender.Send(query, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost("resend")]
    public async Task<IActionResult> Resend(
        [FromBody] ResendReminderRequest request,
        [FromQuery] string? channel = null,
        CancellationToken cancellationToken = default)
    {
        var command = new ResendReminderCommand(request.AppointmentId, channel);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Reminder sent.");
    }
}
