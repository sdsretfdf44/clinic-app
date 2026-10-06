using Arrapa.ClinicOS.API.Controllers;
using Arrapa.ClinicOS.Application.Features.Billing;
using Arrapa.ClinicOS.Contracts.Billing;
using Arrapa.ClinicOS.Shared.Constants;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Billing;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/billing")]
[Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.Receptionist + "," + AppConstants.Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class BillingController : ApiControllerBase
{
    // ==================== Invoices ====================
    [HttpGet("invoices")]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? patientId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListInvoicesQuery(pageNumber, pageSize, patientId, status, from, to, search);
        var result = await Sender.Send(query, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("invoices/{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetInvoiceQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand(
            request.PatientId, request.AppointmentId,
            request.DueAt, request.DiscountAmount, request.TaxAmount,
            request.Currency, request.Notes,
            request.LineItems);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Invoice created.");
    }

    [HttpPut("invoices/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateInvoiceCommand(
            id, request.DueAt, request.DiscountAmount, request.TaxAmount,
            request.Notes, request.LineItems);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Invoice updated.");
    }

    [HttpPost("invoices/{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new IssueInvoiceCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Invoice issued.");
    }

    [HttpPost("invoices/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new CancelInvoiceCommand(id, request.Reason), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Invoice cancelled.");
    }

    [HttpDelete("invoices/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteInvoiceCommand(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Invoice deleted.");
    }

    // ==================== Payments ====================
    [HttpPost("invoices/{id:guid}/payments")]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordPaymentCommand(
            id, request.Amount, request.Method, request.Reference, request.PaidAt, request.Notes);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Payment recorded.");
    }

    [HttpDelete("invoices/{id:guid}/payments/{paymentId:guid}")]
    [Authorize(Roles = AppConstants.Roles.ClinicAdmin + "," + AppConstants.Roles.SuperAdmin)]
    public async Task<IActionResult> VoidPayment(Guid id, Guid paymentId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new VoidPaymentCommand(id, paymentId), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Payment voided.");
    }
}
