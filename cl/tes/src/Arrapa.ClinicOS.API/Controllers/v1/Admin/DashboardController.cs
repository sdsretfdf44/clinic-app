using Arrapa.ClinicOS.Application.Features.Admin.Dashboard.GetSuperAdminDashboard;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Admin;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/dashboard")]
public sealed class DashboardController : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSuperAdminDashboardQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }
}
