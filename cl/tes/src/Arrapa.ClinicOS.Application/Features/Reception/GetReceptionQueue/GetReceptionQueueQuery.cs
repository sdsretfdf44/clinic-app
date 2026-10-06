using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Reception;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Reception.GetReceptionQueue;

/// <summary>
/// Returns today's appointments for the current tenant grouped by reception-relevant status buckets.
/// Optionally filter by <see cref="DoctorId"/> or <see cref="DepartmentId"/> for larger clinics.
/// </summary>
public sealed record GetReceptionQueueQuery(Guid? DoctorId, Guid? DepartmentId) : IRequest<Result<ReceptionQueueResponse>>;

public sealed class GetReceptionQueueQueryHandler
    : IRequestHandler<GetReceptionQueueQuery, Result<ReceptionQueueResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;

    public GetReceptionQueueQueryHandler(IApplicationDbContext db, ITenantContext tenantContext, IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<ReceptionQueueResponse>> Handle(GetReceptionQueueQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<ReceptionQueueResponse>.Failure(tenantIdResult.Error);
        }

        var now = _clock.UtcNow;
        var dayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);

        var query = _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .Include(a => a.Department)
            .Where(a => a.ScheduledStart >= dayStart && a.ScheduledStart < dayEnd)
            .Where(a => a.Status != AppointmentStatus.Cancelled);

        if (request.DoctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == request.DoctorId);
        }
        if (request.DepartmentId.HasValue)
        {
            query = query.Where(a => a.DepartmentId == request.DepartmentId);
        }

        var appointments = await query
            .OrderBy(a => a.ScheduledStart)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var entries = appointments.Select(a => new ReceptionQueueEntry(
            a.Id, a.AppointmentNumber,
            a.PatientId,
            a.Patient?.FullName ?? string.Empty,
            a.Patient?.MRN ?? string.Empty,
            a.Patient?.Phone,
            a.DoctorId,
            a.Doctor?.User?.FullName ?? string.Empty,
            a.Department?.Name,
            a.ScheduledStart, a.ScheduledEnd,
            a.Status.ToString(), a.Reason,
            a.CheckedInAt)).ToList();

        static ReceptionQueueBucket Bucket(IEnumerable<ReceptionQueueEntry> src)
        {
            var list = src.ToList();
            return new ReceptionQueueBucket(list.Count, list);
        }

        return new ReceptionQueueResponse(
            AsOf: now,
            ExpectedToArrive: Bucket(entries.Where(e => e.Status is "Booked" or "Confirmed")),
            WaitingForConsultation: Bucket(entries.Where(e => e.Status == "CheckedIn")),
            InConsultation: Bucket(entries.Where(e => e.Status == "InConsultation")),
            DoneToday: Bucket(entries.Where(e => e.Status == "Completed")),
            NoShow: Bucket(entries.Where(e => e.Status == "NoShow")));
    }
}
