using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Billing;

/// <summary>
/// A billable invoice raised for a patient. Aggregate root — line items and payments belong to this aggregate and
/// can only be mutated through it.
/// </summary>
public sealed class Invoice : BaseEntity, IAggregateRoot
{
    [MaxLength(32)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    /// <summary>Optional link to the appointment that produced this invoice (consultation fee, follow-up, etc.).</summary>
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public DateTimeOffset? IssuedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    /// <summary>Cached sum of payments; kept in sync with <see cref="Payments"/> by the aggregate.</summary>
    public decimal AmountPaid { get; set; }

    /// <summary>Total - AmountPaid. Not stored — derived at read time by callers.</summary>
    public decimal AmountDue => Total - AmountPaid;

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();

    // -------------------- Aggregate operations --------------------
    public void RecalculateTotals()
    {
        Subtotal = LineItems.Sum(l => l.LineTotal);
        Total = Math.Max(0, Subtotal - DiscountAmount + TaxAmount);
    }

    public void RecalculatePaid()
    {
        AmountPaid = Payments.Where(p => !p.IsDeleted).Sum(p => p.Amount);
        Status = (Status, AmountPaid, Total) switch
        {
            (InvoiceStatus.Cancelled, _, _) => InvoiceStatus.Cancelled,
            (InvoiceStatus.Refunded, _, _) => InvoiceStatus.Refunded,
            (_, var paid, var total) when paid <= 0 && Status == InvoiceStatus.Issued => InvoiceStatus.Issued,
            (_, var paid, var total) when paid >= total && total > 0 => InvoiceStatus.Paid,
            (_, var paid, _) when paid > 0 => InvoiceStatus.PartiallyPaid,
            _ => Status
        };
    }

    public bool IsTerminal => Status is InvoiceStatus.Cancelled or InvoiceStatus.Refunded;

    public bool CanEdit => Status == InvoiceStatus.Draft;
    public bool CanPay => Status is InvoiceStatus.Issued or InvoiceStatus.PartiallyPaid;
    public bool CanIssue => Status == InvoiceStatus.Draft && LineItems.Count > 0;
    public bool CanCancel => Status is InvoiceStatus.Draft or InvoiceStatus.Issued or InvoiceStatus.PartiallyPaid;
}
