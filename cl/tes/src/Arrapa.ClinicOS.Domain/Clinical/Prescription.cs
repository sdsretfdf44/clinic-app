using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A prescription document issued at a visit. A visit may have multiple (rare) prescriptions, each with one or
/// more medication line items.
/// </summary>
public sealed class Prescription : BaseEntity
{
    public Guid VisitId { get; set; }
    public Visit? Visit { get; set; }

    public DateTimeOffset IssuedOn { get; set; }

    [MaxLength(2000)]
    public string? GeneralInstructions { get; set; }

    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
