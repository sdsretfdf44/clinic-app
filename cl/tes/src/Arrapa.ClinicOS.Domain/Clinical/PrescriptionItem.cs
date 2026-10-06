using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A single medication line on a prescription.
/// </summary>
public sealed class PrescriptionItem : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }

    [MaxLength(200)]
    public string Medicine { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Strength { get; set; }

    [MaxLength(100)]
    public string? Dosage { get; set; }

    [MaxLength(100)]
    public string? Frequency { get; set; }

    [MaxLength(100)]
    public string? Duration { get; set; }

    [MaxLength(500)]
    public string? Instructions { get; set; }

    public int SortOrder { get; set; }
}
