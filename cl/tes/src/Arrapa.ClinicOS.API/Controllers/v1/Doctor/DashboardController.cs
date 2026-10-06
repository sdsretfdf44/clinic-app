using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Application.Features.Doctors.Dashboard;
using Arrapa.ClinicOS.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Doctor;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/doctor/dashboard")]
[Authorize(Roles = AppConstants.Roles.Doctor + "," + AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class DashboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDoctorDashboardQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }
}
