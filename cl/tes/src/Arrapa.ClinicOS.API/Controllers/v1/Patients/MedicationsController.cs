using Arrapa.ClinicOS.Application.Features.Patients.Medications;
using Arrapa.ClinicOS.Contracts.Patients;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Patients;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patients/{patientId:guid}/medications")]
public sealed class MedicationsController : PatientsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid patientId, [FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListPatientMedicationsQuery(patientId, includeInactive), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid patientId, [FromBody] UpsertPatientMedicationRequest request, CancellationToken cancellationToken)
    {
        var command = new AddPatientMedicationCommand(patientId, request.Name, request.Dosage, request.Frequency, request.StartedOn, request.Notes, request.IsActive);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Medication recorded.");
    }

    [HttpPut("{medicationId:guid}")]
    public async Task<IActionResult> Update(Guid patientId, Guid medicationId, [FromBody] UpsertPatientMedicationRequest request, CancellationToken cancellationToken)
    {
        _ = patientId;
        var command = new UpdatePatientMedicationCommand(medicationId, request.Name, request.Dosage, request.Frequency, request.StartedOn, request.Notes, request.IsActive);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Medication updated.");
    }

    [HttpDelete("{medicationId:guid}")]
    public async Task<IActionResult> Delete(Guid patientId, Guid medicationId, CancellationToken cancellationToken)
    {
        _ = patientId;
        var result = await Sender.Send(new DeletePatientMedicationCommand(medicationId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Medication removed.");
    }
}
