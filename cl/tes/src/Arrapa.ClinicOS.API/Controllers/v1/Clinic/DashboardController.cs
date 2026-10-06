using Arrapa.ClinicOS.Application.Features.Clinic.Dashboard;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clinic/dashboard")]
public sealed class DashboardController : ClinicControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetClinicDashboardQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }
}
