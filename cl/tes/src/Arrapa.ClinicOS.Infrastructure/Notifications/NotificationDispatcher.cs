using Arrapa.ClinicOS.Application.Abstractions.Notifications;
using Arrapa.ClinicOS.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Arrapa.ClinicOS.Infrastructure.Notifications;

/// <summary>
/// Routes an outbound notification to the sender registered for its <see cref="NotificationChannel"/>.
/// Respects the master <see cref="NotificationsOptions.Enabled"/> switch — when disabled, returns a
/// successful "skipped" result so the pipeline still logs deterministic state.
/// </summary>
internal sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly IOptionsMonitor<NotificationsOptions> _optionsMonitor;

    public NotificationDispatcher(
        IEnumerable<INotificationSender> senders,
        IOptionsMonitor<NotificationsOptions> optionsMonitor)
    {
        _senders = senders;
        _optionsMonitor = optionsMonitor;
    }

    public Task<NotificationSendResult> DispatchAsync(
        NotificationChannel channel,
        OutboundNotification message,
        CancellationToken cancellationToken)
    {
        if (!_optionsMonitor.CurrentValue.Enabled)
        {
            return Task.FromResult(NotificationSendResult.Ok(providerMessageId: "disabled"));
        }

        var sender = _senders.FirstOrDefault(s => s.Channel == channel);
        if (sender is null)
        {
            return Task.FromResult(NotificationSendResult.Fail($"No sender registered for channel '{channel}'."));
        }

        return sender.SendAsync(message, cancellationToken);
    }
}
