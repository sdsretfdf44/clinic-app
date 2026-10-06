using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

/// <summary>
/// Error catalog for billing / invoice operations.
/// </summary>
public static class BillingErrors
{
    public static readonly Error NotFound = Error.NotFound("billing.invoice_not_found", "Invoice not found.");
    public static readonly Error PatientNotFound = Error.NotFound("billing.patient_not_found", "Patient not found.");
    public static readonly Error AppointmentNotFound = Error.NotFound("billing.appointment_not_found", "Appointment not found.");
    public static readonly Error PaymentNotFound = Error.NotFound("billing.payment_not_found", "Payment not found.");

    public static readonly Error NoLineItems = Error.Validation("billing.no_line_items", "An invoice must have at least one line item.");
    public static readonly Error InvalidAmount = Error.Validation("billing.invalid_amount", "Amounts must be zero or positive.");
    public static readonly Error InvalidPaymentMethod = Error.Validation("billing.invalid_payment_method", "Unknown payment method.");
    public static readonly Error PaymentTooLarge = Error.Validation("billing.payment_too_large", "Payment exceeds the amount due.");
    public static readonly Error PaymentInFuture = Error.Validation("billing.payment_in_future", "Payment date cannot be in the future.");

    public static readonly Error NotEditable = Error.Conflict("billing.not_editable", "Only draft invoices can be edited.");
    public static readonly Error NotIssuable = Error.Conflict("billing.not_issuable", "Invoice cannot be issued from its current state.");
    public static readonly Error NotPayable = Error.Conflict("billing.not_payable", "Invoice does not accept payments in its current state.");
    public static readonly Error NotCancellable = Error.Conflict("billing.not_cancellable", "Invoice cannot be cancelled from its current state.");
    public static readonly Error Terminal = Error.Conflict("billing.terminal", "Invoice is cancelled or refunded.");
    public static readonly Error AppointmentAlreadyInvoiced = Error.Conflict("billing.appointment_already_invoiced", "This appointment already has an active invoice.");
}
