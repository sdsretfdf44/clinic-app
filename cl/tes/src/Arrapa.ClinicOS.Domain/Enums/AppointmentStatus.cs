namespace Arrapa.ClinicOS.Domain.Enums;

public enum AppointmentStatus
{
    Booked = 1,
    Confirmed = 2,
    CheckedIn = 3,
    InConsultation = 4,
    Completed = 5,
    Cancelled = 6,
    NoShow = 7,
    /// <summary>
    /// A patient-submitted appointment request that has not yet been approved by clinic staff.
    /// Non-binding: does not block staff booking. Transitions to <see cref="Booked"/> on approval or
    /// <see cref="Cancelled"/> on rejection.
    /// </summary>
    Requested = 8
}
