using Arrapa.ClinicOS.Domain.Enums;

namespace Arrapa.ClinicOS.Application.Abstractions.Notifications;

/// <summary>
/// Chooses the right <see cref="INotificationSender"/> for a given channel and delegates to it.
/// Handlers depend on this contract so they don't have to know which providers are wired up.
/// </summary>
public interface INotificationDispatcher
{
    Task<NotificationSendResult> DispatchAsync(
        NotificationChannel channel,
        OutboundNotification message,
        CancellationToken cancellationToken);
}
