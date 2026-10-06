using Arrapa.ClinicOS.Contracts.Billing;
using Arrapa.ClinicOS.Domain.Billing;

namespace Arrapa.ClinicOS.Application.Features.Billing.Common;

internal static class BillingMapping
{
    public static InvoiceSummaryResponse ToSummary(Invoice i) => new(
        i.Id, i.InvoiceNumber,
        i.PatientId, i.Patient?.FullName ?? string.Empty, i.Patient?.MRN ?? string.Empty,
        i.AppointmentId, i.Appointment?.AppointmentNumber,
        i.Status.ToString(),
        i.IssuedAt, i.DueAt,
        i.Subtotal, i.DiscountAmount, i.TaxAmount, i.Total, i.AmountPaid, i.AmountDue,
        i.Currency);

    public static InvoiceDetailResponse ToDetail(Invoice i) => new(
        i.Id, i.InvoiceNumber,
        i.PatientId, i.Patient?.FullName ?? string.Empty, i.Patient?.MRN ?? string.Empty, i.Patient?.Phone,
        i.AppointmentId, i.Appointment?.AppointmentNumber,
        i.Status.ToString(),
        i.IssuedAt, i.DueAt, i.CancelledAt, i.CancellationReason,
        i.Subtotal, i.DiscountAmount, i.TaxAmount, i.Total, i.AmountPaid, i.AmountDue,
        i.Currency, i.Notes,
        i.LineItems.OrderBy(l => l.CreatedOn).Select(ToLineItem).ToList(),
        i.Payments.Where(p => !p.IsDeleted).OrderByDescending(p => p.PaidAt).Select(ToPayment).ToList(),
        i.CreatedOn, i.UpdatedOn,
        i.CanEdit, i.CanIssue, i.CanPay, i.CanCancel);

    public static InvoiceLineItemResponse ToLineItem(InvoiceLineItem l) => new(
        l.Id, l.Description, l.ServiceCode, l.Quantity, l.UnitPrice, l.LineTotal);

    public static InvoicePaymentResponse ToPayment(InvoicePayment p) => new(
        p.Id, p.Amount, p.Method.ToString(), p.Reference, p.PaidAt, p.RecordedByUserId, p.Notes);
}
