using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Application.Features.Portal;
using Arrapa.ClinicOS.Contracts.Portal;
using Arrapa.ClinicOS.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Portal;

/// <summary>
/// Patient portal endpoints — restricted to the <c>Patient</c> role. Every action is scoped to the current user's
/// linked patient record; no PatientId is ever accepted from the client.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/portal")]
[Authorize(Roles = AppConstants.Roles.Patient)]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class PortalController : ApiControllerBase
{
    // ==================== Dashboard ====================
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPortalDashboardQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    // ==================== Profile ====================
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPortalProfileQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePortalProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePortalProfileCommand(
            request.Phone, request.Email, request.PhotoUrl,
            request.AddressLine1, request.AddressLine2, request.City, request.State, request.Country, request.PostalCode,
            request.Occupation,
            request.EmergencyContactName, request.EmergencyContactPhone, request.EmergencyContactRelationship);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Profile updated.");
    }

    // ==================== Doctors directory ====================
    [HttpGet("doctors")]
    public async Task<IActionResult> ListDoctors(
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListPortalDoctorsQuery(departmentId, search), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    // ==================== Appointments ====================
    [HttpGet("appointments")]
    public async Task<IActionResult> ListAppointments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] bool upcomingOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = new ListPortalAppointmentsQuery(pageNumber, pageSize, status, upcomingOnly);
        var result = await Sender.Send(query, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("appointments/{id:guid}")]
    public async Task<IActionResult> GetAppointment(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPortalAppointmentQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost("appointments")]
    public async Task<IActionResult> RequestAppointment([FromBody] RequestAppointmentRequest request, CancellationToken cancellationToken)
    {
        var command = new RequestAppointmentCommand(
            request.DoctorId, request.DepartmentId,
            request.ScheduledStart, request.DurationMinutes, request.Reason);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment requested.");
    }

    [HttpPost("appointments/{id:guid}/cancel")]
    public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] CancelPortalAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new CancelPortalAppointmentCommand(id, request.Reason), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Appointment cancelled.");
    }

    // ==================== Prescriptions ====================
    [HttpGet("prescriptions")]
    public async Task<IActionResult> ListPrescriptions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListPortalPrescriptionsQuery(pageNumber, pageSize), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("prescriptions/{id:guid}")]
    public async Task<IActionResult> GetPrescription(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPortalPrescriptionQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    // ==================== Invoices ====================
    [HttpGet("invoices")]
    public async Task<IActionResult> ListInvoices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListPortalInvoicesQuery(pageNumber, pageSize, status), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("invoices/{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPortalInvoiceQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }
}
