using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Clinical;

/// <summary>
/// A scheduled appointment between a patient and a doctor. Aggregate root — clinical detail added by a doctor at
/// consultation time lives on the separate <c>Visit</c> aggregate (Phase 7).
/// </summary>
public sealed class Appointment : BaseEntity, IAggregateRoot
{
    [MaxLength(32)]
    public string AppointmentNumber { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public DateTimeOffset ScheduledStart { get; set; }
    public DateTimeOffset ScheduledEnd { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    [MaxLength(500)]
    public string? Reason { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Consultation fee snapshot at the moment the appointment was booked. Billing (Phase 9) reads this so future
    /// fee changes don't retroactively re-price historical appointments.
    /// </summary>
    public decimal? ConsultationFee { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    public DateTimeOffset? ConfirmedAt { get; set; }
    public DateTimeOffset? CheckedInAt { get; set; }
    public DateTimeOffset? ConsultationStartedAt { get; set; }
    public DateTimeOffset? ConsultationEndedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    // -------------------- State transition helpers --------------------
    public static readonly IReadOnlySet<AppointmentStatus> TerminalStates =
        new HashSet<AppointmentStatus> { AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow };

    public bool IsTerminal => TerminalStates.Contains(Status);

    public bool CanTransitionTo(AppointmentStatus next) => (Status, next) switch
    {
        (AppointmentStatus.Requested, AppointmentStatus.Booked) => true,
        (AppointmentStatus.Requested, AppointmentStatus.Cancelled) => true,
        (AppointmentStatus.Booked, AppointmentStatus.Confirmed) => true,
        (AppointmentStatus.Booked, AppointmentStatus.CheckedIn) => true,
        (AppointmentStatus.Confirmed, AppointmentStatus.CheckedIn) => true,
        (AppointmentStatus.CheckedIn, AppointmentStatus.InConsultation) => true,
        (AppointmentStatus.InConsultation, AppointmentStatus.Completed) => true,
        (AppointmentStatus.Booked or AppointmentStatus.Confirmed or AppointmentStatus.CheckedIn or AppointmentStatus.InConsultation, AppointmentStatus.Cancelled) => true,
        (AppointmentStatus.Booked or AppointmentStatus.Confirmed, AppointmentStatus.NoShow) => true,
        _ => false
    };
}
