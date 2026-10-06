using Arrapa.ClinicOS.Application.Features.Clinic.Doctors;
using Arrapa.ClinicOS.Contracts.Clinic.Doctors;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clinic/doctors")]
public sealed class DoctorsController : ClinicControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListDoctorsQuery(pageNumber, pageSize, search, departmentId, isActive), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetDoctorQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDoctorCommand(
            request.FirstName, request.LastName, request.Email, request.PhoneNumber,
            request.Specialty, request.LicenseNumber, request.RegistrationNumber,
            request.DepartmentId, request.ConsultationFee, request.Bio, request.IsAcceptingNewPatients);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Doctor created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDoctorCommand(
            id, request.FirstName, request.LastName, request.PhoneNumber, request.AvatarUrl,
            request.Specialty, request.LicenseNumber, request.RegistrationNumber,
            request.DepartmentId, request.ConsultationFee, request.Bio,
            request.IsAcceptingNewPatients, request.SortOrder);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Doctor updated.");
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetDoctorActiveCommand(id, IsActive: true), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Doctor activated.");
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetDoctorActiveCommand(id, IsActive: false), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Doctor deactivated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteDoctorCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Doctor removed.");
    }
}
