using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A past or ongoing medical condition on the patient's timeline. Child of the <see cref="Patient"/> aggregate.
/// </summary>
public sealed class MedicalHistoryEntry : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    [MaxLength(200)]
    public string Condition { get; set; } = string.Empty;

    public DateOnly? Since { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsChronic { get; set; }

    public bool IsResolved { get; set; }
}
