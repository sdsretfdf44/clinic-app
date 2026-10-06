using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

/// <summary>
/// Error catalog for patient-management operations.
/// </summary>
public static class PatientErrors
{
    public static readonly Error NotFound = Error.NotFound("patient.not_found", "Patient not found.");
    public static readonly Error MrnAlreadyExists = Error.Conflict("patient.mrn_exists", "Another patient with the same MRN already exists.");
    public static readonly Error InvalidDateOfBirth = Error.Validation("patient.invalid_dob", "Date of birth cannot be in the future.");
    public static readonly Error InvalidGender = Error.Validation("patient.invalid_gender", "Invalid gender value.");
    public static readonly Error InvalidBloodGroup = Error.Validation("patient.invalid_blood_group", "Invalid blood group value.");
    public static readonly Error InvalidMaritalStatus = Error.Validation("patient.invalid_marital_status", "Invalid marital status value.");

    public static class Allergies
    {
        public static readonly Error NotFound = Error.NotFound("patient.allergy.not_found", "Allergy not found for this patient.");
        public static readonly Error InvalidSeverity = Error.Validation("patient.allergy.invalid_severity", "Invalid allergy severity.");
    }

    public static class MedicalHistory
    {
        public static readonly Error NotFound = Error.NotFound("patient.medical_history.not_found", "Medical history entry not found.");
    }

    public static class Medications
    {
        public static readonly Error NotFound = Error.NotFound("patient.medication.not_found", "Medication not found for this patient.");
    }
}
