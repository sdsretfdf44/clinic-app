using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Application.Features.Reports;
using Arrapa.ClinicOS.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Reports;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reports")]
[Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class ReportsController : ApiControllerBase
{
    [HttpGet("revenue-summary")]
    public async Task<IActionResult> RevenueSummary(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetRevenueSummaryQuery(from, to), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("appointments-funnel")]
    public async Task<IActionResult> AppointmentsFunnel(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetAppointmentsFunnelQuery(from, to), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("top-doctors")]
    public async Task<IActionResult> TopDoctors(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetTopDoctorsReportQuery(from, to, top), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("top-services")]
    public async Task<IActionResult> TopServices(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetTopServicesQuery(from, to, top), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("patient-demographics")]
    public async Task<IActionResult> PatientDemographics(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPatientDemographicsQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }
}
