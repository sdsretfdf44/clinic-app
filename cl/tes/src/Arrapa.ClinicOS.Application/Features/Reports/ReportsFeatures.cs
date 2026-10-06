using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Reports;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Reports;

// ==================== Shared helpers ====================
internal static class ReportsWindow
{
    /// <summary>Clamps + defaults a from/to range. Defaults: last 30 days ending today.</summary>
    public static (DateTimeOffset from, DateTimeOffset to) Resolve(
        DateTimeOffset? from, DateTimeOffset? to, IDateTimeProvider clock)
    {
        var now = clock.UtcNow;
        var toResolved = to ?? new DateTimeOffset(now.Date.AddDays(1), TimeSpan.Zero);
        var fromResolved = from ?? toResolved.AddDays(-30);
        if (fromResolved > toResolved)
        {
            (fromResolved, toResolved) = (toResolved, fromResolved);
        }
        return (fromResolved, toResolved);
    }
}

// ==================== Revenue summary ====================
public sealed record GetRevenueSummaryQuery(DateTimeOffset? From, DateTimeOffset? To)
    : IRequest<Result<RevenueSummaryResponse>>;

public sealed class GetRevenueSummaryQueryHandler
    : IRequestHandler<GetRevenueSummaryQuery, Result<RevenueSummaryResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;

    public GetRevenueSummaryQueryHandler(IApplicationDbContext db, ITenantContext tenantContext, IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<RevenueSummaryResponse>> Handle(GetRevenueSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<RevenueSummaryResponse>.Failure(tenantIdResult.Error);
        }
        var tenantId = tenantIdResult.Value;

        var (from, to) = ReportsWindow.Resolve(request.From, request.To, _clock);

        var currency = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantId && !t.IsDeleted)
            .Select(t => t.Currency)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "USD";

        var invoices = await _db.Invoices.AsNoTracking()
            .Where(i => i.Status != InvoiceStatus.Draft
                     && i.CreatedOn >= from && i.CreatedOn < to)
            .Select(i => new { i.CreatedOn, i.Total, i.AmountPaid, i.Status })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var issuedTotal = invoices.Where(i => i.Status != InvoiceStatus.Cancelled).Sum(i => i.Total);
        var collectedTotal = invoices.Sum(i => i.AmountPaid);
        var outstandingTotal = invoices
            .Where(i => i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Refunded)
            .Sum(i => i.Total - i.AmountPaid);
        var paidCount = invoices.Count(i => i.Status == InvoiceStatus.Paid);

        var series = invoices
            .GroupBy(i => DateOnly.FromDateTime(i.CreatedOn.UtcDateTime))
            .OrderBy(g => g.Key)
            .Select(g => new RevenueSeriesPoint(
                g.Key,
                g.Where(x => x.Status != InvoiceStatus.Cancelled).Sum(x => x.Total),
                g.Sum(x => x.AmountPaid)))
            .ToList();

        return new RevenueSummaryResponse(
            from, to, currency,
            issuedTotal, collectedTotal, outstandingTotal,
            invoices.Count, paidCount,
            series);
    }
}

// ==================== Appointments funnel ====================
public sealed record GetAppointmentsFunnelQuery(DateTimeOffset? From, DateTimeOffset? To)
    : IRequest<Result<AppointmentsFunnelResponse>>;

public sealed class GetAppointmentsFunnelQueryHandler
    : IRequestHandler<GetAppointmentsFunnelQuery, Result<AppointmentsFunnelResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;

    public GetAppointmentsFunnelQueryHandler(IApplicationDbContext db, ITenantContext tenantContext, IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<AppointmentsFunnelResponse>> Handle(GetAppointmentsFunnelQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<AppointmentsFunnelResponse>.Failure(tenantIdResult.Error);
        }

        var (from, to) = ReportsWindow.Resolve(request.From, request.To, _clock);

        var buckets = await _db.Appointments.AsNoTracking()
            .Where(a => a.ScheduledStart >= from && a.ScheduledStart < to)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        int C(AppointmentStatus s) => buckets.FirstOrDefault(b => b.Status == s)?.Count ?? 0;

        var booked = C(AppointmentStatus.Booked);
        var confirmed = C(AppointmentStatus.Confirmed);
        var checkedIn = C(AppointmentStatus.CheckedIn);
        var completed = C(AppointmentStatus.Completed);
        var cancelled = C(AppointmentStatus.Cancelled);
        var noShow = C(AppointmentStatus.NoShow);

        var resolved = completed + cancelled + noShow;
        decimal Pct(int part) => resolved == 0 ? 0m : Math.Round((decimal)part * 100 / resolved, 1);

        return new AppointmentsFunnelResponse(
            from, to,
            booked, confirmed, checkedIn, completed, cancelled, noShow,
            Pct(completed), Pct(noShow), Pct(cancelled));
    }
}

// ==================== Top doctors ====================
public sealed record GetTopDoctorsReportQuery(DateTimeOffset? From, DateTimeOffset? To, int Top)
    : IRequest<Result<TopDoctorsReportResponse>>;

public sealed class GetTopDoctorsReportQueryHandler
    : IRequestHandler<GetTopDoctorsReportQuery, Result<TopDoctorsReportResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;

    public GetTopDoctorsReportQueryHandler(IApplicationDbContext db, ITenantContext tenantContext, IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<TopDoctorsReportResponse>> Handle(GetTopDoctorsReportQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<TopDoctorsReportResponse>.Failure(tenantIdResult.Error);
        }
        var tenantId = tenantIdResult.Value;

        var (from, to) = ReportsWindow.Resolve(request.From, request.To, _clock);
        var top = Math.Clamp(request.Top <= 0 ? 10 : request.Top, 1, 100);

        var currency = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantId && !t.IsDeleted)
            .Select(t => t.Currency)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "USD";

        var appts = await _db.Appointments.AsNoTracking()
            .Where(a => a.ScheduledStart >= from && a.ScheduledStart < to)
            .Select(a => new
            {
                a.DoctorId,
                a.Status,
                DoctorName = a.Doctor!.User!.FirstName + " " + a.Doctor.User.LastName,
                Specialty = a.Doctor.Specialty,
                InvoiceRevenue = _db.Invoices
                    .Where(i => i.AppointmentId == a.Id && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Draft)
                    .Sum(i => (decimal?)i.Total) ?? 0m
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var rows = appts
            .GroupBy(a => new { a.DoctorId, a.DoctorName, a.Specialty })
            .Select(g => new TopDoctorReportRow(
                g.Key.DoctorId,
                g.Key.DoctorName.Trim(),
                g.Key.Specialty,
                g.Count(),
                g.Count(x => x.Status == AppointmentStatus.Completed),
                g.Sum(x => x.InvoiceRevenue)))
            .OrderByDescending(r => r.Revenue)
            .ThenByDescending(r => r.Completed)
            .Take(top)
            .ToList();

        return new TopDoctorsReportResponse(from, to, currency, rows);
    }
}

// ==================== Top services ====================
public sealed record GetTopServicesQuery(DateTimeOffset? From, DateTimeOffset? To, int Top)
    : IRequest<Result<TopServicesReportResponse>>;

public sealed class GetTopServicesQueryHandler
    : IRequestHandler<GetTopServicesQuery, Result<TopServicesReportResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;

    public GetTopServicesQueryHandler(IApplicationDbContext db, ITenantContext tenantContext, IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<TopServicesReportResponse>> Handle(GetTopServicesQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<TopServicesReportResponse>.Failure(tenantIdResult.Error);
        }
        var tenantId = tenantIdResult.Value;

        var (from, to) = ReportsWindow.Resolve(request.From, request.To, _clock);
        var top = Math.Clamp(request.Top <= 0 ? 10 : request.Top, 1, 100);

        var currency = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantId && !t.IsDeleted)
            .Select(t => t.Currency)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "USD";

        var lines = await _db.InvoiceLineItems.AsNoTracking()
            .Where(li => li.Invoice!.CreatedOn >= from && li.Invoice.CreatedOn < to
                      && li.Invoice.Status != InvoiceStatus.Cancelled
                      && li.Invoice.Status != InvoiceStatus.Draft)
            .Select(li => new { li.Description, li.ServiceCode, li.Quantity, li.LineTotal })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var rows = lines
            .GroupBy(l => new
            {
                Description = string.IsNullOrWhiteSpace(l.Description) ? "(unspecified)" : l.Description.Trim(),
                ServiceCode = string.IsNullOrWhiteSpace(l.ServiceCode) ? null : l.ServiceCode!.Trim()
            })
            .Select(g => new TopServiceReportRow(
                g.Key.Description,
                g.Key.ServiceCode,
                g.Count(),
                g.Sum(x => x.Quantity),
                g.Sum(x => x.LineTotal)))
            .OrderByDescending(r => r.Revenue)
            .Take(top)
            .ToList();

        return new TopServicesReportResponse(from, to, currency, rows);
    }
}

// ==================== Patient demographics ====================
public sealed record GetPatientDemographicsQuery : IRequest<Result<PatientDemographicsResponse>>;

public sealed class GetPatientDemographicsQueryHandler
    : IRequestHandler<GetPatientDemographicsQuery, Result<PatientDemographicsResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IDateTimeProvider _clock;

    public GetPatientDemographicsQueryHandler(IApplicationDbContext db, ITenantContext tenantContext, IDateTimeProvider clock)
    {
        _db = db;
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public async Task<Result<PatientDemographicsResponse>> Handle(GetPatientDemographicsQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<PatientDemographicsResponse>.Failure(tenantIdResult.Error);
        }

        var patients = await _db.Patients.AsNoTracking()
            .Select(p => new { p.Gender, p.BloodGroup, p.DateOfBirth })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);

        var genderRows = patients
            .GroupBy(p => p.Gender)
            .Select(g => new GenderBreakdownRow(g.Key.ToString(), g.Count()))
            .OrderByDescending(r => r.Count)
            .ToList();

        var bloodRows = patients
            .Where(p => p.BloodGroup != BloodGroup.Unknown)
            .GroupBy(p => p.BloodGroup)
            .Select(g => new BloodGroupRow(FormatBlood(g.Key), g.Count()))
            .OrderByDescending(r => r.Count)
            .ToList();

        var ageBands = new (string Label, int Min, int Max)[]
        {
            ("0–12", 0, 12),
            ("13–17", 13, 17),
            ("18–29", 18, 29),
            ("30–44", 30, 44),
            ("45–59", 45, 59),
            ("60+", 60, int.MaxValue),
        };

        var ageRows = ageBands.Select(band =>
        {
            var count = patients.Count(p =>
            {
                var age = AgeYears(p.DateOfBirth, today);
                return age >= band.Min && age <= band.Max;
            });
            return new AgeBandRow(band.Label, count);
        }).ToList();

        return new PatientDemographicsResponse(patients.Count, genderRows, ageRows, bloodRows);
    }

    private static int AgeYears(DateOnly dob, DateOnly today)
    {
        var years = today.Year - dob.Year;
        if (dob > today.AddYears(-years)) years--;
        return Math.Max(0, years);
    }

    private static string FormatBlood(BloodGroup bg) => bg switch
    {
        BloodGroup.APositive => "A+",
        BloodGroup.ANegative => "A-",
        BloodGroup.BPositive => "B+",
        BloodGroup.BNegative => "B-",
        BloodGroup.ABPositive => "AB+",
        BloodGroup.ABNegative => "AB-",
        BloodGroup.OPositive => "O+",
        BloodGroup.ONegative => "O-",
        _ => "Unknown"
    };
}
