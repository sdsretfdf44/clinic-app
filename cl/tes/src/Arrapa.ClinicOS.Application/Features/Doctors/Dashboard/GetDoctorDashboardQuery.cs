using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Doctors.Dashboard;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Doctors.Dashboard;

public sealed record GetDoctorDashboardQuery : IRequest<Result<DoctorDashboardResponse>>;

public sealed class GetDoctorDashboardQueryHandler : IRequestHandler<GetDoctorDashboardQuery, Result<DoctorDashboardResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetDoctorDashboardQueryHandler(
        IApplicationDbContext db,
        ITenantContext tenantContext,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<DoctorDashboardResponse>> Handle(GetDoctorDashboardQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure) return Result<DoctorDashboardResponse>.Failure(tenantIdResult.Error);
        var tenantId = tenantIdResult.Value;

        var userId = _currentUser.UserId;
        if (userId is null) return DoctorErrors.NoDoctorRecord;

        var doctor = await _db.Doctors.AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (doctor is null) return DoctorErrors.NoDoctorRecord;

        var currency = await _db.Tenants.IgnoreQueryFilters()
            .Where(t => t.Id == tenantId)
            .Select(t => t.Currency)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "USD";

        var now = _clock.UtcNow;
        var today = new DateTimeOffset(now.Date, TimeSpan.Zero);
        var tomorrow = today.AddDays(1);
        var dow = ((int)now.DayOfWeek + 6) % 7;
        var weekStart = today.AddDays(-dow);
        var weekEnd = weekStart.AddDays(7);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1);
        var last30Start = today.AddDays(-29);

        // -------------------- Today --------------------
        var todayRows = await _db.Appointments.AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id
                && a.ScheduledStart >= today && a.ScheduledStart < tomorrow
                && a.Status != AppointmentStatus.Requested)
            .Select(a => new { a.Status, a.ScheduledStart })
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var todayTotal = todayRows.Count(r => r.Status != AppointmentStatus.Cancelled);
        var todayInConsult = todayRows.Count(r => r.Status == AppointmentStatus.InConsultation);
        var todayCompleted = todayRows.Count(r => r.Status == AppointmentStatus.Completed);
        var todayNoShow = todayRows.Count(r => r.Status == AppointmentStatus.NoShow);
        var todayRemaining = todayRows.Count(r =>
            r.Status is AppointmentStatus.Booked or AppointmentStatus.Confirmed or AppointmentStatus.CheckedIn);
        var todayKpis = new DoctorTodayKpis(todayTotal, todayRemaining, todayInConsult, todayCompleted, todayNoShow);

        var weekKpis = await AggregateRangeAsync(doctor.Id, weekStart, weekEnd, cancellationToken).ConfigureAwait(false);
        var monthKpis = await AggregateRangeAsync(doctor.Id, monthStart, monthEnd, cancellationToken).ConfigureAwait(false);

        // -------------------- Lifetime --------------------
        var totalCompleted = await _db.Appointments.CountAsync(a =>
            a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Completed, cancellationToken).ConfigureAwait(false);
        var uniquePatients = await _db.Appointments.AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Completed)
            .Select(a => a.PatientId).Distinct()
            .CountAsync(cancellationToken).ConfigureAwait(false);

        // -------------------- Next appointments (upcoming, next 5) --------------------
        var nextRows = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctor.Id
                && a.ScheduledStart >= now
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && a.Status != AppointmentStatus.Completed
                && a.Status != AppointmentStatus.Requested)
            .OrderBy(a => a.ScheduledStart)
            .Take(5)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var next = nextRows.Select(a => new DoctorAppointmentPreview(
            a.Id, a.AppointmentNumber,
            a.PatientId, a.Patient?.FullName ?? string.Empty, a.Patient?.MRN ?? string.Empty,
            a.ScheduledStart, a.ScheduledEnd,
            a.Status.ToString(), a.Reason)).ToList();

        // -------------------- Trend (last 30 days, own only) --------------------
        var trendRows = await _db.Appointments.AsNoTracking()
            .Where(a => a.DoctorId == doctor.Id
                && a.ScheduledStart >= last30Start && a.ScheduledStart < tomorrow
                && a.Status != AppointmentStatus.Requested)
            .Select(a => new { a.ScheduledStart, a.Status })
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var trendMap = new Dictionary<DateOnly, (int total, int completed)>();
        for (int i = 0; i < 30; i++)
        {
            var d = DateOnly.FromDateTime(last30Start.UtcDateTime.AddDays(i));
            trendMap[d] = (0, 0);
        }
        foreach (var r in trendRows)
        {
            var d = DateOnly.FromDateTime(r.ScheduledStart.UtcDateTime);
            if (!trendMap.TryGetValue(d, out var current)) continue;
            var completedDelta = r.Status == AppointmentStatus.Completed ? 1 : 0;
            var totalDelta = r.Status == AppointmentStatus.Cancelled ? 0 : 1;
            trendMap[d] = (current.total + totalDelta, current.completed + completedDelta);
        }
        var trend = trendMap.OrderBy(kv => kv.Key)
            .Select(kv => new DoctorTrendPoint(kv.Key, kv.Value.total, kv.Value.completed))
            .ToList();

        return new DoctorDashboardResponse(
            new DoctorProfileSummary(
                doctor.Id,
                doctor.User?.FullName ?? string.Empty,
                doctor.Specialty,
                doctor.DepartmentId,
                doctor.Department?.Name,
                currency),
            todayKpis,
            weekKpis,
            monthKpis,
            new DoctorLifetimeKpis(uniquePatients, totalCompleted),
            next,
            trend);
    }

    private async Task<DoctorRangeKpis> AggregateRangeAsync(Guid doctorId, DateTimeOffset start, DateTimeOffset endExclusive, CancellationToken cancellationToken)
    {
        var rows = await _db.Appointments.AsNoTracking()
            .Where(a => a.DoctorId == doctorId
                && a.ScheduledStart >= start && a.ScheduledStart < endExclusive
                && a.Status != AppointmentStatus.Requested)
            .Select(a => new { a.Status, a.ConsultationFee })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var booked = rows.Count(r => r.Status != AppointmentStatus.Cancelled);
        var completed = rows.Count(r => r.Status == AppointmentStatus.Completed);
        var cancelled = rows.Count(r => r.Status == AppointmentStatus.Cancelled);
        var noShow = rows.Count(r => r.Status == AppointmentStatus.NoShow);
        var revenue = rows.Where(r => r.Status == AppointmentStatus.Completed).Sum(r => r.ConsultationFee ?? 0m);

        return new DoctorRangeKpis(booked, completed, cancelled, noShow, revenue);
    }
}
