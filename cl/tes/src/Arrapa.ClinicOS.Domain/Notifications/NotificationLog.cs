using System.ComponentModel.DataAnnotations;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Common;
using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Domain.Notifications;

/// <summary>
/// A single delivery attempt for an outbound notification (WhatsApp, SMS, or email).
/// One row per (Appointment, Kind) — deduplication is enforced at write time so the reminder scheduler
/// can safely re-run without spamming recipients.
/// </summary>
public sealed class NotificationLog : BaseEntity, IAggregateRoot
{
    /// <summary>Appointment this notification refers to. Nullable so non-appointment notifications can be logged later.</summary>
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public NotificationKind Kind { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    /// <summary>Phone number (E.164) or email address the message was sent to.</summary>
    [MaxLength(200)]
    public string Recipient { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Body { get; set; } = string.Empty;

    /// <summary>Provider message id (e.g., Twilio SID) — useful for correlating with provider dashboards.</summary>
    [MaxLength(100)]
    public string? ProviderMessageId { get; set; }

    public DateTimeOffset? SentAt { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}
