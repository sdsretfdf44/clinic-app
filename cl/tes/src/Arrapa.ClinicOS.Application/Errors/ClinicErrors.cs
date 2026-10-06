using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

/// <summary>
/// Error catalog for clinic-admin operations.
/// </summary>
public static class ClinicErrors
{
    public static readonly Error NoTenantContext = Error.Unauthorized(
        "clinic.no_tenant", "A tenant context is required for this operation.");

    public static class Departments
    {
        public static readonly Error NotFound = Error.NotFound("clinic.department.not_found", "Department not found.");
        public static readonly Error NameExists = Error.Conflict("clinic.department.name_exists", "Another department with the same name already exists.");
        public static readonly Error HasDoctors = Error.Conflict("clinic.department.has_doctors", "This department has active doctors and cannot be deleted.");
    }

    public static class Doctors
    {
        public static readonly Error NotFound = Error.NotFound("clinic.doctor.not_found", "Doctor not found.");
        public static readonly Error EmailInUse = Error.Conflict("clinic.doctor.email_exists", "Another user in this clinic already uses this email.");
    }

    public static class Receptionists
    {
        public static readonly Error NotFound = Error.NotFound("clinic.receptionist.not_found", "Receptionist not found.");
        public static readonly Error EmailInUse = Error.Conflict("clinic.receptionist.email_exists", "Another user in this clinic already uses this email.");
    }

    public static class Users
    {
        public static readonly Error NotFound = Error.NotFound("clinic.user.not_found", "User not found in this clinic.");
        public static readonly Error CannotChangeOwnRole = Error.Forbidden("clinic.user.cannot_change_own_role", "You cannot change your own role.");
        public static readonly Error CannotDeactivateSelf = Error.Forbidden("clinic.user.cannot_deactivate_self", "You cannot deactivate your own account.");
        public static readonly Error InvalidRole = Error.Validation("clinic.user.invalid_role", "Invalid role.");
        public static readonly Error EmailInUse = Error.Conflict("clinic.user.email_exists", "Another user in this clinic already uses this email.");
    }

    public static class WorkingHours
    {
        public static readonly Error DoctorNotFound = Error.NotFound("clinic.working_hours.doctor_not_found", "Doctor not found in this clinic.");
        public static readonly Error InvalidSlot = Error.Validation("clinic.working_hours.invalid_slot", "Start time must be earlier than end time.");
    }

    public static class Settings
    {
        public static readonly Error NotFound = Error.NotFound("clinic.setting.not_found", "Setting not found.");
    }
}
