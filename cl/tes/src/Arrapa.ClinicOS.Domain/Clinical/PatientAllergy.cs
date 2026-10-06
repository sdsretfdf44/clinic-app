using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A known allergy for a patient. Child of the <see cref="Patient"/> aggregate.
/// </summary>
public sealed class PatientAllergy : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public AllergySeverity Severity { get; set; } = AllergySeverity.Unknown;

    [MaxLength(500)]
    public string? ReactionDescription { get; set; }

    public DateOnly? IdentifiedOn { get; set; }
}
