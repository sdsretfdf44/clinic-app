using Arrapa.ClinicOS.Application.Features.Admin.FeatureFlags.ClearOverride;
using Arrapa.ClinicOS.Application.Features.Admin.FeatureFlags.ListFeatureFlags;
using Arrapa.ClinicOS.Application.Features.Admin.FeatureFlags.SetOverride;
using Arrapa.ClinicOS.Application.Features.Admin.FeatureFlags.UpsertFeatureFlag;
using Arrapa.ClinicOS.Contracts.Admin.FeatureFlags;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Admin;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/feature-flags")]
public sealed class FeatureFlagsController : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ListFeatureFlagsQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var command = new UpsertFeatureFlagCommand(null, request.Key, request.Name, request.Description, request.DefaultEnabled, request.IsGlobal);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Feature flag created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var command = new UpsertFeatureFlagCommand(id, request.Key, request.Name, request.Description, request.DefaultEnabled, request.IsGlobal);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Feature flag updated.");
    }

    [HttpPost("{id:guid}/override")]
    public async Task<IActionResult> SetOverride(Guid id, [FromBody] SetFeatureOverrideRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetOverrideCommand(id, request.TenantId, request.IsEnabled), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Override applied.");
    }

    [HttpDelete("{id:guid}/override/{tenantId:guid}")]
    public async Task<IActionResult> ClearOverride(Guid id, Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ClearOverrideCommand(id, tenantId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Override cleared.");
    }
}
