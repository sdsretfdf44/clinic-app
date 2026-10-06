using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Notifications;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Notifications;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Notifications;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Notifications;

// ==================== List logs ====================
public sealed record ListNotificationLogsQuery(
    int PageNumber, int PageSize,
    Guid? AppointmentId,
    string? Kind, string? Channel, string? Status,
    string? Search)
    : IRequest<Result<PaginatedList<NotificationLogResponse>>>;

public sealed class ListNotificationLogsQueryHandler
    : IRequestHandler<ListNotificationLogsQuery, Result<PaginatedList<NotificationLogResponse>>>
{
    private readonly IApplicationDbContext _db;
    public ListNotificationLogsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<PaginatedList<NotificationLogResponse>>> Handle(
        ListNotificationLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.NotificationLogs.AsNoTracking()
            .Include(n => n.Appointment)!.ThenInclude(a => a!.Patient)
            .AsQueryable();

        if (request.AppointmentId.HasValue)
            query = query.Where(n => n.AppointmentId == request.AppointmentId);

        if (!string.IsNullOrWhiteSpace(request.Kind) &&
            Enum.TryParse<NotificationKind>(request.Kind, ignoreCase: true, out var k))
            query = query.Where(n => n.Kind == k);

        if (!string.IsNullOrWhiteSpace(request.Channel) &&
            Enum.TryParse<NotificationChannel>(request.Channel, ignoreCase: true, out var ch))
            query = query.Where(n => n.Channel == ch);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<NotificationStatus>(request.Status, ignoreCase: true, out var st))
            query = query.Where(n => n.Status == st);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(n =>
                n.Recipient.ToLower().Contains(term) ||
                n.Body.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var rows = await query
            .OrderByDescending(n => n.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var items = rows.Select(n => new NotificationLogResponse(
            n.Id,
            n.AppointmentId,
            n.Appointment?.AppointmentNumber,
            n.Appointment?.Patient == null ? null :
                string.Join(' ', new[] { n.Appointment.Patient.FirstName, n.Appointment.Patient.LastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s))),
            n.Kind.ToString(),
            n.Channel.ToString(),
            n.Status.ToString(),
            n.Recipient,
            n.Body,
            n.ProviderMessageId,
            n.ErrorMessage,
            n.SentAt,
            n.CreatedOn)).ToList();

        return new PaginatedList<NotificationLogResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

// ==================== Manual resend ====================
public sealed record ResendReminderCommand(Guid AppointmentId, string? Channel)
    : IRequest<Result<NotificationLogResponse>>;

public sealed class ResendReminderCommandHandler
    : IRequestHandler<ResendReminderCommand, Result<NotificationLogResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly INotificationDispatcher _dispatcher;
    private readonly ITenantContext _tenantContext;

    public ResendReminderCommandHandler(
        IApplicationDbContext db,
        INotificationDispatcher dispatcher,
        ITenantContext tenantContext)
    {
        _db = db;
        _dispatcher = dispatcher;
        _tenantContext = tenantContext;
    }

    public async Task<Result<NotificationLogResponse>> Handle(
        ResendReminderCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
            return Result<NotificationLogResponse>.Failure(tenantIdResult.Error);

        var appointment = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken).ConfigureAwait(false);
        if (appointment is null)
            return NotificationErrors.AppointmentNotFound;

        var recipient = appointment.Patient?.Phone;
        if (string.IsNullOrWhiteSpace(recipient))
            return NotificationErrors.NoRecipientPhone;

        var channel = Enum.TryParse<NotificationChannel>(request.Channel, ignoreCase: true, out var parsed)
            ? parsed
            : NotificationChannel.WhatsApp;

        var tenantName = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantIdResult.Value)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "your clinic";

        var doctor = appointment.Doctor?.User is null
            ? "your doctor"
            : $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}".Trim();

        var body = BuildManualBody(tenantName, appointment.Patient?.FirstName ?? string.Empty, doctor, appointment.ScheduledStart);

        var result = await _dispatcher.DispatchAsync(channel, new OutboundNotification(recipient, body), cancellationToken)
            .ConfigureAwait(false);

        var log = new NotificationLog
        {
            TenantId = tenantIdResult.Value,
            AppointmentId = appointment.Id,
            Kind = NotificationKind.ManualResend,
            Channel = channel,
            Status = result.Success ? NotificationStatus.Sent : NotificationStatus.Failed,
            Recipient = recipient,
            Body = body,
            ProviderMessageId = result.ProviderMessageId,
            ErrorMessage = result.ErrorMessage,
            SentAt = result.Success ? DateTimeOffset.UtcNow : null,
        };
        _db.NotificationLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var patientName = appointment.Patient == null ? null :
            string.Join(' ', new[] { appointment.Patient.FirstName, appointment.Patient.LastName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        return new NotificationLogResponse(
            log.Id, log.AppointmentId, appointment.AppointmentNumber, patientName,
            log.Kind.ToString(), log.Channel.ToString(), log.Status.ToString(),
            log.Recipient, log.Body, log.ProviderMessageId, log.ErrorMessage,
            log.SentAt, DateTimeOffset.UtcNow);
    }

    private static string BuildManualBody(string clinic, string firstName, string doctor, DateTimeOffset start)
    {
        var name = string.IsNullOrWhiteSpace(firstName) ? "there" : firstName;
        var when = start.ToString("dddd, dd MMM yyyy 'at' hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
        return $"Hi {name}, reminder: your appointment with Dr. {doctor} at {clinic} is scheduled for {when}.";
    }
}
