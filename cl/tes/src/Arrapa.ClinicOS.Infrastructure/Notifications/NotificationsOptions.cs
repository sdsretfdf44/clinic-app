namespace Arrapa.ClinicOS.Infrastructure.Notifications;

/// <summary>
/// Root configuration for outbound notifications. Bind from the "Notifications" section of appsettings.json.
/// </summary>
public sealed class NotificationsOptions
{
    public const string SectionName = "Notifications";

    /// <summary>Master switch. When false, all sends are recorded as <c>Skipped</c>.</summary>
    public bool Enabled { get; set; }

    /// <summary>How often the reminder scheduler polls for pending reminders. Defaults to 5 minutes.</summary>
    public int SchedulerIntervalSeconds { get; set; } = 300;

    /// <summary>Send a booking confirmation shortly after appointment creation.</summary>
    public bool SendBookingConfirmation { get; set; } = true;

    /// <summary>Send the ~24h-out reminder.</summary>
    public bool SendReminder24h { get; set; } = true;

    /// <summary>Send the ~1h-out reminder.</summary>
    public bool SendReminder1h { get; set; } = true;

    /// <summary>Default channel to use when a template doesn't specify one.</summary>
    public string DefaultChannel { get; set; } = "WhatsApp";

    public WhatsAppProviderOptions WhatsApp { get; set; } = new();
    public SmsProviderOptions Sms { get; set; } = new();
}

public sealed class WhatsAppProviderOptions
{
    /// <summary>Currently supported: "Twilio". Anything else (or empty) selects the log-only fallback.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>Twilio account SID.</summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>Twilio auth token.</summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>WhatsApp sender in E.164 form without the "whatsapp:" prefix (e.g. "+14155238886").</summary>
    public string FromNumber { get; set; } = string.Empty;
}

public sealed class SmsProviderOptions
{
    /// <summary>Currently supported: "Twilio". Anything else (or empty) selects the log-only fallback.</summary>
    public string Provider { get; set; } = string.Empty;

    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>SMS sender in E.164 form (e.g. "+14155238886").</summary>
    public string FromNumber { get; set; } = string.Empty;
}
