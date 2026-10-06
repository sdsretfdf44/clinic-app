using Arrapa.ClinicOS.Application.Features.Admin.Plans.CreatePlan;
using Arrapa.ClinicOS.Application.Features.Admin.Plans.ListPlans;
using Arrapa.ClinicOS.Application.Features.Admin.Plans.UpdatePlan;
using Arrapa.ClinicOS.Contracts.Admin.Plans;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Admin;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/plans")]
public sealed class SubscriptionPlansController : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListPlansQuery(includeInactive), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePlanCommand(
            request.Code, request.Name, request.Description, request.Interval, request.Price, request.Currency,
            request.MaxUsers, request.MaxPatients, request.MaxAppointmentsPerMonth,
            request.Features, request.IsActive, request.IsDefault, request.SortOrder);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Plan created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePlanCommand(
            id, request.Code, request.Name, request.Description, request.Interval, request.Price, request.Currency,
            request.MaxUsers, request.MaxPatients, request.MaxAppointmentsPerMonth,
            request.Features, request.IsActive, request.IsDefault, request.SortOrder);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Plan updated.");
    }
}
