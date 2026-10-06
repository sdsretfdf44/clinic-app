using Arrapa.ClinicOS.Application.Features.Clinic.WorkingHours;
using Arrapa.ClinicOS.Contracts.Clinic.WorkingHours;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clinic/working-hours")]
public sealed class WorkingHoursController : ClinicControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetClinicHours(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetWorkingHoursQuery(DoctorId: null), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> SetClinicHours([FromBody] SetWorkingHoursRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetWorkingHoursCommand(DoctorId: null, request.Slots), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Clinic working hours saved.");
    }

    [HttpGet("doctors/{doctorId:guid}")]
    public async Task<IActionResult> GetDoctorHours(Guid doctorId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetWorkingHoursQuery(doctorId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPut("doctors/{doctorId:guid}")]
    public async Task<IActionResult> SetDoctorHours(Guid doctorId, [FromBody] SetWorkingHoursRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetWorkingHoursCommand(doctorId, request.Slots), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Doctor working hours saved.");
    }
}
