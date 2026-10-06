using Arrapa.ClinicOS.Application.Abstractions.Notifications;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Notifications;
using Arrapa.ClinicOS.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arrapa.ClinicOS.Infrastructure.Notifications;

/// <summary>
/// Background service that periodically scans appointments and dispatches booking confirmations plus
/// 24h / 1h reminders. Uses <see cref="NotificationLog"/> as an idempotency ledger so restarts and
/// overlapping ticks never double-send.
/// </summary>
internal sealed class AppointmentReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<NotificationsOptions> _optionsMonitor;
    private readonly ILogger<AppointmentReminderService> _logger;

    public AppointmentReminderService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<NotificationsOptions> optionsMonitor,
        ILogger<AppointmentReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppointmentReminderService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _optionsMonitor.CurrentValue;
            var interval = TimeSpan.FromSeconds(Math.Max(30, options.SchedulerIntervalSeconds));

            try
            {
                await RunOnceAsync(options, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AppointmentReminderService tick failed. Backing off for {Interval}.", interval);
            }

            try
            {
                await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("AppointmentReminderService stopping.");
    }

    private async Task RunOnceAsync(NotificationsOptions options, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var channel = ParseChannel(options.DefaultChannel);
        var now = clock.UtcNow;

        // Confirmation: appointments created in the last 30 minutes.
        if (options.SendBookingConfirmation)
        {
            await ProcessKindAsync(db, dispatcher, channel, NotificationKind.BookingConfirmation,
                now.AddMinutes(-30), now.AddYears(1),
                requireCreatedAfter: now.AddMinutes(-30),
                cancellationToken).ConfigureAwait(false);
        }

        // 24h reminder: appointments starting in [23h, 25h) from now.
        if (options.SendReminder24h)
        {
            await ProcessKindAsync(db, dispatcher, channel, NotificationKind.Reminder24h,
                now.AddHours(23), now.AddHours(25),
                requireCreatedAfter: null,
                cancellationToken).ConfigureAwait(false);
        }

        // 1h reminder: appointments starting in [30m, 90m) from now.
        if (options.SendReminder1h)
        {
            await ProcessKindAsync(db, dispatcher, channel, NotificationKind.Reminder1h,
                now.AddMinutes(30), now.AddMinutes(90),
                requireCreatedAfter: null,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task ProcessKindAsync(
        IApplicationDbContext db,
        INotificationDispatcher dispatcher,
        NotificationChannel channel,
        NotificationKind kind,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        DateTimeOffset? requireCreatedAfter,
        CancellationToken cancellationToken)
    {
        var query = db.Appointments.AsNoTracking().IgnoreQueryFilters()
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .Where(a => !a.IsDeleted
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Status != AppointmentStatus.NoShow
                        && a.Status != AppointmentStatus.Completed
                        && a.ScheduledStart >= windowStart
                        && a.ScheduledStart < windowEnd);

        if (requireCreatedAfter is { } createdAfter)
        {
            query = query.Where(a => a.CreatedOn >= createdAfter);
        }

        var candidates = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        if (candidates.Count == 0)
        {
            return;
        }

        var candidateIds = candidates.Select(a => a.Id).ToList();
        var alreadySent = await db.NotificationLogs.AsNoTracking().IgnoreQueryFilters()
            .Where(l => l.AppointmentId != null
                        && candidateIds.Contains(l.AppointmentId.Value)
                        && l.Kind == kind)
            .Select(l => l.AppointmentId!.Value)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var sentSet = alreadySent.ToHashSet();

        var tenantSlugs = await db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => !t.IsDeleted)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var tenantName = tenantSlugs.ToDictionary(t => t.Id, t => t.Name);

        foreach (var appt in candidates)
        {
            if (sentSet.Contains(appt.Id) || cancellationToken.IsCancellationRequested)
            {
                continue;
            }

            var recipient = appt.Patient?.Phone;
            if (string.IsNullOrWhiteSpace(recipient))
            {
                // Record a Skipped row so we don't retry on every tick.
                await WriteLogAsync(db, appt.TenantId, appt.Id, kind, channel,
                    recipient: appt.Patient?.Phone ?? "(no phone)",
                    body: "(no recipient phone on patient record)",
                    status: NotificationStatus.Skipped,
                    providerMessageId: null,
                    errorMessage: "Patient has no phone number.",
                    sentAt: null,
                    cancellationToken).ConfigureAwait(false);
                continue;
            }

            var clinic = tenantName.TryGetValue(appt.TenantId, out var name) ? name : "your clinic";
            var doctor = appt.Doctor?.User is null
                ? "your doctor"
                : $"{appt.Doctor.User.FirstName} {appt.Doctor.User.LastName}".Trim();
            var firstName = appt.Patient?.FirstName ?? string.Empty;
            var body = AppointmentMessageTemplates.Build(kind, clinic, firstName, doctor, appt.ScheduledStart);

            var result = await dispatcher.DispatchAsync(channel, new OutboundNotification(recipient, body), cancellationToken)
                .ConfigureAwait(false);

            await WriteLogAsync(db, appt.TenantId, appt.Id, kind, channel,
                recipient: recipient,
                body: body,
                status: result.Success ? NotificationStatus.Sent : NotificationStatus.Failed,
                providerMessageId: result.ProviderMessageId,
                errorMessage: result.ErrorMessage,
                sentAt: result.Success ? DateTimeOffset.UtcNow : null,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WriteLogAsync(
        IApplicationDbContext db,
        Guid tenantId,
        Guid appointmentId,
        NotificationKind kind,
        NotificationChannel channel,
        string recipient,
        string body,
        NotificationStatus status,
        string? providerMessageId,
        string? errorMessage,
        DateTimeOffset? sentAt,
        CancellationToken cancellationToken)
    {
        var log = new NotificationLog
        {
            TenantId = tenantId,
            AppointmentId = appointmentId,
            Kind = kind,
            Channel = channel,
            Recipient = recipient,
            Body = Truncate(body, 2000),
            Status = status,
            ProviderMessageId = providerMessageId,
            ErrorMessage = errorMessage,
            SentAt = sentAt,
        };

        db.NotificationLogs.Add(log);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static NotificationChannel ParseChannel(string value) =>
        Enum.TryParse<NotificationChannel>(value, ignoreCase: true, out var parsed)
            ? parsed
            : NotificationChannel.WhatsApp;

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
