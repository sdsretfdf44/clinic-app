using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Identity;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A doctor practicing at the clinic. Backed by an <see cref="ApplicationUser"/> record so the doctor can also
/// sign in and use the Doctor portal in later phases.
/// </summary>
public sealed class Doctor : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    [MaxLength(200)]
    public string? Specialty { get; set; }

    [MaxLength(64)]
    public string? LicenseNumber { get; set; }

    [MaxLength(64)]
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Overrides <see cref="Department.DefaultConsultationFee"/> when set.
    /// </summary>
    public decimal? ConsultationFee { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public bool IsAcceptingNewPatients { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
}
