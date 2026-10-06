using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Patients;

/// <summary>
/// Shared authorization + serialization defaults for all patient endpoints.
/// </summary>
[ApiController]
[Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.Doctor + "," + AppConstants.Roles.Receptionist + "," + AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class PatientsControllerBase : ApiControllerBase
{
}
