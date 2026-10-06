using Arrapa.ClinicOS.Application.Features.Patients.Allergies;
using Arrapa.ClinicOS.Contracts.Patients;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Patients;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patients/{patientId:guid}/allergies")]
public sealed class AllergiesController : PatientsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ListPatientAllergiesQuery(patientId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid patientId, [FromBody] UpsertPatientAllergyRequest request, CancellationToken cancellationToken)
    {
        var command = new AddPatientAllergyCommand(patientId, request.Name, request.Severity, request.ReactionDescription, request.IdentifiedOn);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Allergy recorded.");
    }

    [HttpPut("{allergyId:guid}")]
    public async Task<IActionResult> Update(Guid patientId, Guid allergyId, [FromBody] UpsertPatientAllergyRequest request, CancellationToken cancellationToken)
    {
        _ = patientId;
        var command = new UpdatePatientAllergyCommand(allergyId, request.Name, request.Severity, request.ReactionDescription, request.IdentifiedOn);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Allergy updated.");
    }

    [HttpDelete("{allergyId:guid}")]
    public async Task<IActionResult> Delete(Guid patientId, Guid allergyId, CancellationToken cancellationToken)
    {
        _ = patientId;
        var result = await Sender.Send(new DeletePatientAllergyCommand(allergyId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Allergy removed.");
    }
}
