using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

/// <summary>
/// Error catalog for appointment operations.
/// </summary>
public static class AppointmentErrors
{
    public static readonly Error NotFound = Error.NotFound("appointment.not_found", "Appointment not found.");
    public static readonly Error PatientNotFound = Error.NotFound("appointment.patient_not_found", "Patient not found.");
    public static readonly Error DoctorNotFound = Error.NotFound("appointment.doctor_not_found", "Doctor not found.");
    public static readonly Error DoctorInactive = Error.Conflict("appointment.doctor_inactive", "Doctor is not active.");
    public static readonly Error PatientInactive = Error.Conflict("appointment.patient_inactive", "Patient is not active.");
    public static readonly Error DepartmentNotFound = Error.NotFound("appointment.department_not_found", "Department not found.");
    public static readonly Error InvalidTime = Error.Validation("appointment.invalid_time", "Appointment end time must be after start time.");
    public static readonly Error InPast = Error.Validation("appointment.in_past", "Appointments cannot be scheduled in the past.");
    public static readonly Error Overlap = Error.Conflict("appointment.overlap", "The doctor already has another appointment during this time.");
    public static readonly Error IllegalTransition = Error.Conflict("appointment.illegal_transition", "This status change is not permitted from the current state.");
    public static readonly Error Terminal = Error.Conflict("appointment.terminal", "This appointment is already completed, cancelled, or marked as no-show.");
    public static readonly Error InvalidDuration = Error.Validation("appointment.invalid_duration", "Duration must be between 5 and 480 minutes.");
}
