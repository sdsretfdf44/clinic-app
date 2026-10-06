using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

/// <summary>
/// Error catalog for patient-portal operations.
/// </summary>
public static class PortalErrors
{
    public static readonly Error NoPatientRecord = Error.NotFound(
        "portal.no_patient_record",
        "No patient record is linked to this account. Please contact your clinic.");

    public static readonly Error AppointmentNotFound = Error.NotFound(
        "portal.appointment_not_found",
        "Appointment not found.");

    public static readonly Error InvoiceNotFound = Error.NotFound(
        "portal.invoice_not_found",
        "Invoice not found.");

    public static readonly Error PrescriptionNotFound = Error.NotFound(
        "portal.prescription_not_found",
        "Prescription not found.");

    public static readonly Error CannotCancel = Error.Conflict(
        "portal.cannot_cancel",
        "This appointment can no longer be cancelled.");
}
