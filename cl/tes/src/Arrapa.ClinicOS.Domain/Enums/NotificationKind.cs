namespace Arrapa.ClinicOS.Domain.Enums;

public enum NotificationKind
{
    /// <summary>Sent shortly after the appointment is booked to confirm details.</summary>
    BookingConfirmation = 1,
    /// <summary>Sent roughly 24 hours before the appointment start time.</summary>
    Reminder24h = 2,
    /// <summary>Sent roughly 1 hour before the appointment start time.</summary>
    Reminder1h = 3,
    /// <summary>Explicitly triggered by clinic staff via the resend endpoint.</summary>
    ManualResend = 4,
}
