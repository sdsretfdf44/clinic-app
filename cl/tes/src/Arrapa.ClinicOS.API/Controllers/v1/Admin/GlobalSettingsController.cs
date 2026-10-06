using Arrapa.ClinicOS.Application.Features.Admin.Settings.ListSettings;
using Arrapa.ClinicOS.Application.Features.Admin.Settings.UpsertSetting;
using Arrapa.ClinicOS.Contracts.Admin.Settings;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Admin;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/settings")]
public sealed class GlobalSettingsController : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? category = null, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListSettingsQuery(category), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertSettingRequest request, CancellationToken cancellationToken)
    {
        var command = new UpsertSettingCommand(request.Key, request.Value, request.Category, request.DataType, request.Description, request.IsSecret);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Setting saved.");
    }
}
