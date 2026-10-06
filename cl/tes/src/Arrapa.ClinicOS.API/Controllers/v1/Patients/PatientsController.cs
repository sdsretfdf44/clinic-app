using Arrapa.ClinicOS.Application.Features.Patients.Patients;
using Arrapa.ClinicOS.Contracts.Patients;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Patients;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patients")]
public sealed class PatientsController : PatientsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? gender = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListPatientsQuery(pageNumber, pageSize, search, gender, isActive), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPatientQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePatientCommand(
            request.FirstName, request.MiddleName, request.LastName,
            request.DateOfBirth, request.Gender, request.BloodGroup, request.MaritalStatus,
            request.Phone, request.Email, request.PhotoUrl,
            request.AddressLine1, request.AddressLine2, request.City, request.State, request.Country, request.PostalCode,
            request.Occupation,
            request.EmergencyContactName, request.EmergencyContactPhone, request.EmergencyContactRelationship,
            request.GuardianName, request.GuardianPhone, request.GuardianRelationship,
            request.InsuranceProvider, request.InsurancePolicyNumber, request.InsuranceGroupNumber, request.InsuranceValidUntil,
            request.Notes);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Patient registered.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePatientCommand(
            id, request.FirstName, request.MiddleName, request.LastName,
            request.DateOfBirth, request.Gender, request.BloodGroup, request.MaritalStatus,
            request.Phone, request.Email, request.PhotoUrl,
            request.AddressLine1, request.AddressLine2, request.City, request.State, request.Country, request.PostalCode,
            request.Occupation,
            request.EmergencyContactName, request.EmergencyContactPhone, request.EmergencyContactRelationship,
            request.GuardianName, request.GuardianPhone, request.GuardianRelationship,
            request.InsuranceProvider, request.InsurancePolicyNumber, request.InsuranceGroupNumber, request.InsuranceValidUntil,
            request.Notes);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Patient updated.");
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetPatientActiveCommand(id, IsActive: true), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Patient activated.");
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetPatientActiveCommand(id, IsActive: false), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Patient deactivated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeletePatientCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Patient removed.");
    }
}
