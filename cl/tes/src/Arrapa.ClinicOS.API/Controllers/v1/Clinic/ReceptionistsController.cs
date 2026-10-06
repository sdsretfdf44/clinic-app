using Arrapa.ClinicOS.Application.Features.Clinic.Receptionists;
using Arrapa.ClinicOS.Contracts.Clinic.Receptionists;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clinic/receptionists")]
public sealed class ReceptionistsController : ClinicControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool includeInactive = true, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListReceptionistsQuery(includeInactive), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReceptionistRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReceptionistCommand(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Title);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Receptionist added.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReceptionistRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateReceptionistCommand(id, request.FirstName, request.LastName, request.PhoneNumber, request.AvatarUrl, request.Title, request.SortOrder);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Receptionist updated.");
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetReceptionistActiveCommand(id, IsActive: true), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Receptionist activated.");
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetReceptionistActiveCommand(id, IsActive: false), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Receptionist deactivated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteReceptionistCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Receptionist removed.");
    }
}
