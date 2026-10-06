namespace Arrapa.ClinicOS.Contracts.Notifications;

public sealed record NotificationLogResponse(
    Guid Id,
    Guid? AppointmentId,
    string? AppointmentNumber,
    string? PatientName,
    string Kind,
    string Channel,
    string Status,
    string Recipient,
    string Body,
    string? ProviderMessageId,
    string? ErrorMessage,
    DateTimeOffset? SentAt,
    DateTimeOffset CreatedOn);

public sealed record ResendReminderRequest(Guid AppointmentId);
