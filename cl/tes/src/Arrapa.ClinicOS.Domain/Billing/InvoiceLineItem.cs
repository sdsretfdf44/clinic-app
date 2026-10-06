using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Billing;

/// <summary>
/// A single billable line on an <see cref="Invoice"/>. Belongs to the invoice aggregate.
/// </summary>
public sealed class InvoiceLineItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Optional service/procedure code (e.g., CPT or an internal SKU).</summary>
    [MaxLength(50)]
    public string? ServiceCode { get; set; }

    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }

    /// <summary>Quantity * UnitPrice, stored so listing doesn't re-multiply on every read.</summary>
    public decimal LineTotal { get; set; }
}
