using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Application.Features.Appointments;
using Arrapa.ClinicOS.Contracts.Appointments;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Appointments;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/appointments")]
[Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.Doctor + "," + AppConstants.Roles.Receptionist + "," + AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class AppointmentsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListAppointmentsQuery(pageNumber, pageSize, from, to, doctorId, patientId, departmentId, status, search);
        var result = await Sender.Send(query, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAppointmentQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        var command = new BookAppointmentCommand(
            request.PatientId, request.DoctorId, request.DepartmentId,
            request.ScheduledStart, request.DurationMinutes, request.Reason, request.Notes);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment booked.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        var command = new RescheduleAppointmentCommand(
            id, request.DoctorId, request.DepartmentId,
            request.ScheduledStart, request.DurationMinutes, request.Reason, request.Notes);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment rescheduled.");
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new TransitionAppointmentCommand(id, AppointmentStatus.Booked), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment request approved.");
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new TransitionAppointmentCommand(id, AppointmentStatus.Confirmed), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment confirmed.");
    }

    [HttpPost("{id:guid}/check-in")]
    public async Task<IActionResult> CheckIn(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new TransitionAppointmentCommand(id, AppointmentStatus.CheckedIn), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Patient checked in.");
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> StartConsultation(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new TransitionAppointmentCommand(id, AppointmentStatus.InConsultation), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Consultation started.");
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new TransitionAppointmentCommand(id, AppointmentStatus.Completed), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment completed.");
    }

    [HttpPost("{id:guid}/no-show")]
    public async Task<IActionResult> NoShow(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new TransitionAppointmentCommand(id, AppointmentStatus.NoShow), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Marked as no-show.");
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new CancelAppointmentCommand(id, request.Reason), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment cancelled.");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteAppointmentCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment deleted.");
    }

    [HttpGet("available-slots")]
    public async Task<IActionResult> AvailableSlots(
        [FromQuery] Guid doctorId,
        [FromQuery] DateOnly date,
        [FromQuery] int slotMinutes = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new AvailableSlotsQuery(doctorId, date, slotMinutes), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }
}
