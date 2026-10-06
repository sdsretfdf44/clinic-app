using Arrapa.ClinicOS.Application.Features.Clinic.Departments;
using Arrapa.ClinicOS.Contracts.Clinic.Departments;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clinic/departments")]
public sealed class DepartmentsController : ClinicControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool includeInactive = true, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListDepartmentsQuery(includeInactive), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDepartmentQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertDepartmentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request.Name, request.Description, request.DefaultConsultationFee, request.IsActive, request.SortOrder);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Department created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertDepartmentRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentCommand(id, request.Name, request.Description, request.DefaultConsultationFee, request.IsActive, request.SortOrder);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Department updated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteDepartmentCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Department deleted.");
    }
}
