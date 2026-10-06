using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Clinic.Dashboard;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Dashboard;

public sealed record GetClinicDashboardQuery : IRequest<Result<ClinicDashboardResponse>>;

public sealed class GetClinicDashboardQueryHandler : IRequestHandler<GetClinicDashboardQuery, Result<ClinicDashboardResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;

    public GetClinicDashboardQueryHandler(IApplicationDbContext db, ITenantContext tenantContext, IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<ClinicDashboardResponse>> Handle(GetClinicDashboardQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<ClinicDashboardResponse>.Failure(tenantIdResult.Error);
        }
        var tenantId = tenantIdResult.Value;

        var tenant = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantId && !t.IsDeleted)
            .Select(t => new { t.Currency, t.DefaultConsultationFee })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return ClinicErrors.NoTenantContext;
        }

        // -------------------- Staff KPIs --------------------
        var totalUsers = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenantId && !u.IsDeleted, cancellationToken).ConfigureAwait(false);
        var activeUsers = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenantId && !u.IsDeleted && u.IsActive, cancellationToken).ConfigureAwait(false);
        var doctors = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenantId && !u.IsDeleted && u.PrimaryRole == UserRole.Doctor, cancellationToken).ConfigureAwait(false);
        var receptionists = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenantId && !u.IsDeleted && u.PrimaryRole == UserRole.Receptionist, cancellationToken).ConfigureAwait(false);
        var admins = await _db.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenantId && !u.IsDeleted && u.PrimaryRole == UserRole.ClinicAdmin, cancellationToken).ConfigureAwait(false);

        // -------------------- Departments --------------------
        var totalDepartments = await _db.Departments.CountAsync(cancellationToken).ConfigureAwait(false);
        var activeDepartments = await _db.Departments.CountAsync(d => d.IsActive, cancellationToken).ConfigureAwait(false);

        var breakdown = await _db.Departments.AsNoTracking()
            .OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
            .Select(d => new DepartmentSummary(
                d.Id, d.Name,
                _db.Doctors.Count(doc => doc.DepartmentId == d.Id),
                d.DefaultConsultationFee))
            .Take(10)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // -------------------- Time windows --------------------
        var now = _clock.UtcNow;
        var today = new DateTimeOffset(now.Date, TimeSpan.Zero);
        var tomorrow = today.AddDays(1);
        var dayAfterTomorrow = today.AddDays(2);
        // ISO week starts Monday. Shift so Sunday (0) becomes 6.
        var dow = ((int)now.DayOfWeek + 6) % 7;
        var weekStart = today.AddDays(-dow);
        var weekEnd = weekStart.AddDays(7);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var monthEnd = monthStart.AddMonths(1);
        var last30Start = today.AddDays(-29);

        // -------------------- Appointment KPIs --------------------
        var todayCount = await _db.Appointments.CountAsync(a =>
            a.ScheduledStart >= today && a.ScheduledStart < tomorrow
            && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Requested,
            cancellationToken).ConfigureAwait(false);

        var tomorrowCount = await _db.Appointments.CountAsync(a =>
            a.ScheduledStart >= tomorrow && a.ScheduledStart < dayAfterTomorrow
            && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Requested,
            cancellationToken).ConfigureAwait(false);

        var weekCount = await _db.Appointments.CountAsync(a =>
            a.ScheduledStart >= weekStart && a.ScheduledStart < weekEnd
            && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Requested,
            cancellationToken).ConfigureAwait(false);

        var monthCount = await _db.Appointments.CountAsync(a =>
            a.ScheduledStart >= monthStart && a.ScheduledStart < monthEnd
            && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Requested,
            cancellationToken).ConfigureAwait(false);

        var pendingRequests = await _db.Appointments.CountAsync(a =>
            a.Status == AppointmentStatus.Requested, cancellationToken).ConfigureAwait(false);

        // No-show rate: last 30 days, resolved (not requested, not still-scheduled-in-future) appointments.
        var resolvedLast30 = await _db.Appointments.CountAsync(a =>
            a.ScheduledStart >= last30Start && a.ScheduledStart < tomorrow
            && a.Status != AppointmentStatus.Requested,
            cancellationToken).ConfigureAwait(false);
        var noShowLast30 = await _db.Appointments.CountAsync(a =>
            a.ScheduledStart >= last30Start && a.ScheduledStart < tomorrow
            && a.Status == AppointmentStatus.NoShow,
            cancellationToken).ConfigureAwait(false);
        var noShowRate = resolvedLast30 == 0 ? 0m : Math.Round((decimal)noShowLast30 * 100 / resolvedLast30, 1);

        // -------------------- Revenue KPIs --------------------
        var invoicesThisMonth = await _db.Invoices.AsNoTracking()
            .Where(i => i.CreatedOn >= monthStart && i.CreatedOn < monthEnd && i.Status != InvoiceStatus.Draft)
            .Select(i => new { i.Total, i.AmountPaid, i.Status })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var issuedThisMonth = invoicesThisMonth
            .Where(i => i.Status != InvoiceStatus.Cancelled)
            .Sum(i => i.Total);

        // Collected = payments recorded this month (across all invoices, not just those created this month).
        var collectedThisMonth = await _db.InvoicePayments.AsNoTracking()
            .Where(p => !p.IsDeleted && p.PaidAt >= monthStart && p.PaidAt < monthEnd)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken).ConfigureAwait(false) ?? 0m;

        var outstandingList = await _db.Invoices.AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid)
            .Select(i => new { i.Total, i.AmountPaid })
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var outstandingTotal = outstandingList.Sum(i => i.Total - i.AmountPaid);
        var outstandingCount = outstandingList.Count;

        // -------------------- Patient KPIs --------------------
        var totalActivePatients = await _db.Patients.CountAsync(p => p.IsActive, cancellationToken).ConfigureAwait(false);
        var newPatientsThisMonth = await _db.Patients.CountAsync(
            p => p.CreatedOn >= monthStart && p.CreatedOn < monthEnd, cancellationToken).ConfigureAwait(false);

        // -------------------- Trend (30 days) --------------------
        var trendRows = await _db.Appointments.AsNoTracking()
            .Where(a => a.ScheduledStart >= last30Start && a.ScheduledStart < tomorrow
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
        var trend = trendMap
            .OrderBy(kv => kv.Key)
            .Select(kv => new AppointmentTrendPoint(kv.Key, kv.Value.total, kv.Value.completed))
            .ToList();

        // -------------------- Top doctors this month --------------------
        var topDoctorRows = await _db.Appointments.AsNoTracking()
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .Where(a => a.ScheduledStart >= monthStart && a.ScheduledStart < monthEnd
                && a.Status == AppointmentStatus.Completed)
            .GroupBy(a => new { a.DoctorId, DoctorName = a.Doctor!.User!.FirstName + " " + a.Doctor.User.LastName, Specialty = a.Doctor.Specialty })
            .Select(g => new
            {
                g.Key.DoctorId,
                g.Key.DoctorName,
                g.Key.Specialty,
                Completed = g.Count(),
                Revenue = g.Sum(a => a.ConsultationFee ?? 0m),
            })
            .OrderByDescending(x => x.Completed)
            .Take(5)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var topDoctors = topDoctorRows
            .Select(r => new TopDoctorSummary(r.DoctorId, r.DoctorName.Trim(), r.Specialty, r.Completed, r.Revenue))
            .ToList();

        return new ClinicDashboardResponse(
            new ClinicStaffKpis(totalUsers, activeUsers, doctors, receptionists, admins),
            new ClinicDepartmentKpis(totalDepartments, activeDepartments),
            new ClinicAppointmentKpis(todayCount, tomorrowCount, weekCount, monthCount, pendingRequests, noShowRate),
            new ClinicRevenueKpis(issuedThisMonth, collectedThisMonth, outstandingTotal, outstandingCount),
            new ClinicPatientKpis(totalActivePatients, newPatientsThisMonth),
            tenant.Currency,
            tenant.DefaultConsultationFee,
            breakdown,
            trend,
            topDoctors);
    }
}
