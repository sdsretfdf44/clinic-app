using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Application.Features.Visits;
using Arrapa.ClinicOS.Contracts.Visits;
using Arrapa.ClinicOS.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Visits;

/// <summary>
/// Clinical authoring endpoints for visits. Doctors and super-admins can mutate; clinic admins and receptionists
/// can read.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/visits")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class VisitsController : ApiControllerBase
{
    // ---------- Read ----------
    [HttpGet("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.Doctor + "," + AppConstants.Roles.Receptionist + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetVisitQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("by-appointment/{appointmentId:guid}")]
    [Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.Doctor + "," + AppConstants.Roles.Receptionist + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> GetByAppointment(Guid appointmentId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetVisitByAppointmentQuery(appointmentId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    // ---------- Vitals ----------
    [HttpPut("{id:guid}/vitals")]
    [Authorize(Roles = AppConstants.Roles.Doctor + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> UpsertVitals(Guid id, [FromBody] UpsertVitalsRequest request, CancellationToken cancellationToken)
    {
        var command = new UpsertVitalsCommand(
            id,
            request.BloodPressureSystolic, request.BloodPressureDiastolic,
            request.PulseBpm, request.TemperatureCelsius,
            request.HeightCm, request.WeightKg,
            request.SpO2Percent, request.RespirationRate, request.Notes);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Vitals saved.");
    }

    // ---------- Diagnosis ----------
    [HttpPut("{id:guid}/diagnosis")]
    [Authorize(Roles = AppConstants.Roles.Doctor + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> UpsertDiagnosis(Guid id, [FromBody] UpsertDiagnosisRequest request, CancellationToken cancellationToken)
    {
        var command = new UpsertDiagnosisCommand(id, request.Symptoms, request.Diagnosis, request.DoctorNotes);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Notes saved.");
    }

    // ---------- Prescriptions ----------
    [HttpPost("{id:guid}/prescriptions")]
    [Authorize(Roles = AppConstants.Roles.Doctor + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> AddPrescription(Guid id, [FromBody] UpsertPrescriptionRequest request, CancellationToken cancellationToken)
    {
        var items = request.Items.Select(i => new PrescriptionItemInput(
            i.Medicine, i.Strength, i.Dosage, i.Frequency, i.Duration, i.Instructions, i.SortOrder)).ToList();
        var command = new AddPrescriptionCommand(id, request.GeneralInstructions, items);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Prescription added.");
    }

    [HttpPut("prescriptions/{prescriptionId:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> UpdatePrescription(Guid prescriptionId, [FromBody] UpsertPrescriptionRequest request, CancellationToken cancellationToken)
    {
        var items = request.Items.Select(i => new PrescriptionItemInput(
            i.Medicine, i.Strength, i.Dosage, i.Frequency, i.Duration, i.Instructions, i.SortOrder)).ToList();
        var command = new UpdatePrescriptionCommand(prescriptionId, request.GeneralInstructions, items);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Prescription updated.");
    }

    [HttpDelete("prescriptions/{prescriptionId:guid}")]
    [Authorize(Roles = AppConstants.Roles.Doctor + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> DeletePrescription(Guid prescriptionId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeletePrescriptionCommand(prescriptionId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Prescription removed.");
    }
}
