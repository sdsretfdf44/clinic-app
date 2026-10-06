using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Application.Abstractions.Notifications;

/// <summary>
/// Outbound notification transport. Infrastructure supplies WhatsApp / SMS implementations; a no-op logger
/// serves as the default when no provider is configured.
/// </summary>
public interface INotificationSender
{
    NotificationChannel Channel { get; }

    Task<NotificationSendResult> SendAsync(OutboundNotification message, CancellationToken cancellationToken);
}

public sealed record OutboundNotification(string Recipient, string Body);

public sealed record NotificationSendResult(bool Success, string? ProviderMessageId, string? ErrorMessage)
{
    public static NotificationSendResult Ok(string? providerMessageId = null) =>
        new(true, providerMessageId, null);
    public static NotificationSendResult Fail(string errorMessage) =>
        new(false, null, errorMessage);
}
