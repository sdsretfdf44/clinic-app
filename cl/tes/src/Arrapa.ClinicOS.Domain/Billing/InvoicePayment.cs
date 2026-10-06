using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Billing;

/// <summary>
/// A payment received against an <see cref="Invoice"/>. Belongs to the invoice aggregate.
/// </summary>
public sealed class InvoicePayment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public decimal Amount { get; set; }

    public PaymentMode Method { get; set; }

    /// <summary>Optional external reference (transaction id, cheque number, UPI ref, etc.).</summary>
    [MaxLength(100)]
    public string? Reference { get; set; }

    public DateTimeOffset PaidAt { get; set; }

    public Guid RecordedByUserId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
