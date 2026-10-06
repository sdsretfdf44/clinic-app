using Arrapa.ClinicOS.Shared.Results;

namespace Arrapa.ClinicOS.Application.Errors;

public static class NotificationErrors
{
    public static readonly Error AppointmentNotFound = Error.NotFound(
        "notification.appointment_not_found", "Appointment not found.");

    public static readonly Error NoRecipientPhone = Error.Validation(
        "notification.no_recipient_phone", "Patient does not have a phone number on file.");
}
