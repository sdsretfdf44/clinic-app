using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A clinical department or specialty grouping inside a clinic (e.g., Cardiology, General Medicine).
/// Tenant-scoped aggregate root.
/// </summary>
public sealed class Department : BaseEntity, IAggregateRoot
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Optional default consultation fee for doctors in this department. Doctor-level fee overrides this.
    /// </summary>
    public decimal? DefaultConsultationFee { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
