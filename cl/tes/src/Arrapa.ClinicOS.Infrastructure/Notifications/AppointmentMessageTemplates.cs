using System.Globalization;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Infrastructure.Notifications;

internal static class AppointmentMessageTemplates
{
    public static string Build(
        NotificationKind kind,
        string clinicName,
        string patientFirstName,
        string doctorName,
        DateTimeOffset scheduledStart)
    {
        var localTime = scheduledStart.ToString("dddd, dd MMM yyyy 'at' hh:mm tt", CultureInfo.InvariantCulture);
        var name = string.IsNullOrWhiteSpace(patientFirstName) ? "there" : patientFirstName;

        return kind switch
        {
            NotificationKind.BookingConfirmation =>
                $"Hi {name}, your appointment with Dr. {doctorName} at {clinicName} is confirmed for {localTime}. " +
                "Reply STOP to opt out.",

            NotificationKind.Reminder24h =>
                $"Hi {name}, this is a reminder: you have an appointment with Dr. {doctorName} at {clinicName} tomorrow ({localTime}). " +
                "See you then.",

            NotificationKind.Reminder1h =>
                $"Hi {name}, your appointment with Dr. {doctorName} at {clinicName} starts in about an hour ({localTime}). " +
                "Please arrive 10 minutes early.",

            NotificationKind.ManualResend =>
                $"Hi {name}, reminder: your appointment with Dr. {doctorName} at {clinicName} is scheduled for {localTime}.",

            _ => $"Hi {name}, appointment update for {localTime} with Dr. {doctorName} at {clinicName}."
        };
    }
}
