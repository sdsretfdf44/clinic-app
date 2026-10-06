using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Admin;

/// <summary>
/// Base class for all super-admin controllers. Enforces the <c>SuperAdmin</c> role at the API layer so the auth
/// pipeline short-circuits before hitting business logic.
/// </summary>
[ApiController]
[Authorize(Roles = AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class AdminControllerBase : ApiControllerBase
{
}
