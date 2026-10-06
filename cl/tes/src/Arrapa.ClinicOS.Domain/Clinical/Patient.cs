using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Identity;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A patient at the clinic. Aggregate root: owns allergies, medical history entries, and current medications.
/// Uniquely identified within a tenant by its <see cref="MRN"/> (Medical Record Number).
/// </summary>
public sealed class Patient : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Optional link to an <see cref="ApplicationUser"/> so the patient can sign in and use the Patient Portal.
    /// Nullable because clinic staff can register patients who never create an online account.
    /// </summary>
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [MaxLength(32)]
    public string MRN { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => string.Join(' ',
        new[] { FirstName, MiddleName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));

    public DateOnly DateOfBirth { get; set; }

    public Gender Gender { get; set; } = Gender.Unknown;

    public BloodGroup BloodGroup { get; set; } = BloodGroup.Unknown;

    public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    // -------------------- Address --------------------
    [MaxLength(200)] public string? AddressLine1 { get; set; }
    [MaxLength(200)] public string? AddressLine2 { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    [MaxLength(20)]  public string? PostalCode { get; set; }

    [MaxLength(200)] public string? Occupation { get; set; }

    // -------------------- Emergency contact --------------------
    [MaxLength(200)] public string? EmergencyContactName { get; set; }
    [MaxLength(30)]  public string? EmergencyContactPhone { get; set; }
    [MaxLength(100)] public string? EmergencyContactRelationship { get; set; }

    // -------------------- Guardian (for minors) --------------------
    [MaxLength(200)] public string? GuardianName { get; set; }
    [MaxLength(30)]  public string? GuardianPhone { get; set; }
    [MaxLength(100)] public string? GuardianRelationship { get; set; }

    // -------------------- Insurance --------------------
    [MaxLength(200)] public string? InsuranceProvider { get; set; }
    [MaxLength(100)] public string? InsurancePolicyNumber { get; set; }
    [MaxLength(100)] public string? InsuranceGroupNumber { get; set; }
    public DateOnly? InsuranceValidUntil { get; set; }

    [MaxLength(4000)] public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastVisitAt { get; set; }

    public ICollection<PatientAllergy> Allergies { get; set; } = new List<PatientAllergy>();
    public ICollection<MedicalHistoryEntry> MedicalHistory { get; set; } = new List<MedicalHistoryEntry>();
    public ICollection<PatientMedication> CurrentMedications { get; set; } = new List<PatientMedication>();
}
