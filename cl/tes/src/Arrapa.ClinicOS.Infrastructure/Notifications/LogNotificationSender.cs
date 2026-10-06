using Arrapa.ClinicOS.Application.Abstractions.Notifications;
using Arrapa.ClinicOS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Infrastructure.Notifications;

/// <summary>
/// Fallback notification sender that just logs the message. Used when no real provider is configured or when
/// <see cref="NotificationsOptions.Enabled"/> is false. Lets developers exercise the reminder pipeline without
/// wiring a paid provider.
/// </summary>
internal sealed class LogNotificationSender : INotificationSender
{
    private readonly ILogger<LogNotificationSender> _logger;
    private readonly NotificationChannel _channel;

    public LogNotificationSender(ILogger<LogNotificationSender> logger, NotificationChannel channel)
    {
        _logger = logger;
        _channel = channel;
    }

    public NotificationChannel Channel => _channel;

    public Task<NotificationSendResult> SendAsync(OutboundNotification message, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[NOTIFY:{Channel}] to={Recipient} body={Body}",
            _channel, message.Recipient, message.Body);

        return Task.FromResult(NotificationSendResult.Ok(providerMessageId: $"log-{Guid.NewGuid():N}"));
    }
}
