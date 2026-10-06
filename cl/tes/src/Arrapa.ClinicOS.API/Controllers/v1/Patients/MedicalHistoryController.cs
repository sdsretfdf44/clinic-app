using Arrapa.ClinicOS.Application.Features.Patients.MedicalHistory;
using Arrapa.ClinicOS.Contracts.Patients;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Patients;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patients/{patientId:guid}/medical-history")]
public sealed class MedicalHistoryController : PatientsControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ListMedicalHistoryQuery(patientId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Guid patientId, [FromBody] UpsertMedicalHistoryEntryRequest request, CancellationToken cancellationToken)
    {
        var command = new AddMedicalHistoryCommand(patientId, request.Condition, request.Since, request.Notes, request.IsChronic, request.IsResolved);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "History entry recorded.");
    }

    [HttpPut("{entryId:guid}")]
    public async Task<IActionResult> Update(Guid patientId, Guid entryId, [FromBody] UpsertMedicalHistoryEntryRequest request, CancellationToken cancellationToken)
    {
        _ = patientId;
        var command = new UpdateMedicalHistoryCommand(entryId, request.Condition, request.Since, request.Notes, request.IsChronic, request.IsResolved);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "History entry updated.");
    }

    [HttpDelete("{entryId:guid}")]
    public async Task<IActionResult> Delete(Guid patientId, Guid entryId, CancellationToken cancellationToken)
    {
        _ = patientId;
        var result = await Sender.Send(new DeleteMedicalHistoryCommand(entryId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "History entry removed.");
    }
}
