using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A medication the patient is currently taking (or has taken). Prescriptions issued at consultations are a
/// separate aggregate introduced in Phase 10; this entity captures the intake list volunteered at registration.
/// </summary>
public sealed class PatientMedication : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Dosage { get; set; }

    [MaxLength(100)]
    public string? Frequency { get; set; }

    public DateOnly? StartedOn { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
