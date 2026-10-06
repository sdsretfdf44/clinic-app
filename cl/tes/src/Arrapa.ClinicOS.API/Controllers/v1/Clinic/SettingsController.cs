using Arrapa.ClinicOS.Application.Features.Clinic.Settings;
using Arrapa.ClinicOS.Contracts.Clinic.Settings;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clinic/settings")]
public sealed class SettingsController : ClinicControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? category = null, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListClinicSettingsQuery(category), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertClinicSettingRequest request, CancellationToken cancellationToken)
    {
        var command = new UpsertClinicSettingCommand(request.Key, request.Value, request.Category, request.DataType, request.Description, request.IsSecret);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Setting saved.");
    }
}
