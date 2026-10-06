using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A clinical encounter — the doctor-authored record of what happened during (or in place of) an appointment.
/// One visit per appointment; auto-created when an appointment enters <c>InConsultation</c>. Owns vitals and
/// prescriptions.
/// </summary>
public sealed class Visit : BaseEntity, IAggregateRoot
{
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public DateTimeOffset VisitDate { get; set; }

    [MaxLength(2000)]
    public string? Symptoms { get; set; }

    [MaxLength(2000)]
    public string? Diagnosis { get; set; }

    [MaxLength(4000)]
    public string? DoctorNotes { get; set; }

    /// <summary>
    /// Once the underlying appointment is Completed, the visit becomes read-only. SuperAdmin escape hatch is
    /// intentionally deferred to a later phase.
    /// </summary>
    public bool IsLocked { get; set; }

    public VisitVitals? Vitals { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
