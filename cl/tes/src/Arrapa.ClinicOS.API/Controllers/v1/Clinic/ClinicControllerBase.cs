using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

/// <summary>
/// Base class for all clinic-admin controllers. Grants access to both <c>ClinicAdmin</c> and <c>SuperAdmin</c>
/// so super-admins can manage any tenant they've resolved via <c>X-Tenant-Id</c>.
/// </summary>
[ApiController]
[Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class ClinicControllerBase : ApiControllerBase
{
}
