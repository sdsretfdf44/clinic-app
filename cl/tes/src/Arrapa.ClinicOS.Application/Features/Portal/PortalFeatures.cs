using Arrapa.ClinicOS.Application.Abstractions.Appointments;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Portal.Common;
using Arrapa.ClinicOS.Contracts.Portal;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Portal;

// ==================== Profile ====================
public sealed record GetPortalProfileQuery : IRequest<Result<PortalProfileResponse>>;

public sealed class GetPortalProfileQueryHandler : IRequestHandler<GetPortalProfileQuery, Result<PortalProfileResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetPortalProfileQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<PortalProfileResponse>> Handle(GetPortalProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var patient = await _db.Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (patient is null) return PortalErrors.NoPatientRecord;

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        return PortalMapping.ToProfile(patient, today);
    }
}

public sealed record UpdatePortalProfileCommand(
    string? Phone,
    string? Email,
    string? PhotoUrl,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string? Occupation,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelationship) : IRequest<Result<PortalProfileResponse>>;

public sealed class UpdatePortalProfileCommandValidator : AbstractValidator<UpdatePortalProfileCommand>
{
    public UpdatePortalProfileCommandValidator()
    {
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.PhotoUrl).MaximumLength(500);
        RuleFor(x => x.AddressLine1).MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Occupation).MaximumLength(200);
        RuleFor(x => x.EmergencyContactName).MaximumLength(200);
        RuleFor(x => x.EmergencyContactPhone).MaximumLength(30);
        RuleFor(x => x.EmergencyContactRelationship).MaximumLength(100);
    }
}

public sealed class UpdatePortalProfileCommandHandler : IRequestHandler<UpdatePortalProfileCommand, Result<PortalProfileResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public UpdatePortalProfileCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<PortalProfileResponse>> Handle(UpdatePortalProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (patient is null) return PortalErrors.NoPatientRecord;

        patient.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        patient.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        patient.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();
        patient.AddressLine1 = string.IsNullOrWhiteSpace(request.AddressLine1) ? null : request.AddressLine1.Trim();
        patient.AddressLine2 = string.IsNullOrWhiteSpace(request.AddressLine2) ? null : request.AddressLine2.Trim();
        patient.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
        patient.State = string.IsNullOrWhiteSpace(request.State) ? null : request.State.Trim();
        patient.Country = string.IsNullOrWhiteSpace(request.Country) ? null : request.Country.Trim();
        patient.PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? null : request.PostalCode.Trim();
        patient.Occupation = string.IsNullOrWhiteSpace(request.Occupation) ? null : request.Occupation.Trim();
        patient.EmergencyContactName = string.IsNullOrWhiteSpace(request.EmergencyContactName) ? null : request.EmergencyContactName.Trim();
        patient.EmergencyContactPhone = string.IsNullOrWhiteSpace(request.EmergencyContactPhone) ? null : request.EmergencyContactPhone.Trim();
        patient.EmergencyContactRelationship = string.IsNullOrWhiteSpace(request.EmergencyContactRelationship) ? null : request.EmergencyContactRelationship.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        return PortalMapping.ToProfile(patient, today);
    }
}

// ==================== Doctor directory ====================
public sealed record ListPortalDoctorsQuery(Guid? DepartmentId, string? Search) : IRequest<Result<IReadOnlyList<PortalDoctorResponse>>>;

public sealed class ListPortalDoctorsQueryHandler : IRequestHandler<ListPortalDoctorsQuery, Result<IReadOnlyList<PortalDoctorResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ListPortalDoctorsQueryHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<PortalDoctorResponse>>> Handle(ListPortalDoctorsQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure) return Result<IReadOnlyList<PortalDoctorResponse>>.Failure(tenantIdResult.Error);

        var currency = await _db.Tenants.IgnoreQueryFilters()
            .Where(t => t.Id == tenantIdResult.Value)
            .Select(t => t.Currency)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(currency)) currency = "USD";

        var query = _db.Doctors.AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.IsActive && d.IsAcceptingNewPatients);

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == request.DepartmentId);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(d =>
                (d.User != null && (d.User.FirstName + " " + d.User.LastName).ToLower().Contains(term)) ||
                (d.Specialty != null && d.Specialty.ToLower().Contains(term)));
        }

        var doctors = await query
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.User!.LastName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        IReadOnlyList<PortalDoctorResponse> result = doctors.Select(d => PortalMapping.ToDoctor(d, currency!)).ToList();
        return Result<IReadOnlyList<PortalDoctorResponse>>.Success(result);
    }
}

// ==================== Appointments — list & get ====================
public sealed record ListPortalAppointmentsQuery(
    int PageNumber, int PageSize, string? Status, bool UpcomingOnly)
    : IRequest<Result<PaginatedList<PortalAppointmentSummaryResponse>>>;

public sealed class ListPortalAppointmentsQueryHandler
    : IRequestHandler<ListPortalAppointmentsQuery, Result<PaginatedList<PortalAppointmentSummaryResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public ListPortalAppointmentsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<PaginatedList<PortalAppointmentSummaryResponse>>> Handle(
        ListPortalAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var patientId = await _db.Patients.AsNoTracking()
            .Where(p => p.UserId == userId).Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (patientId is null) return PortalErrors.NoPatientRecord;

        var query = _db.Appointments.AsNoTracking()
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .Include(a => a.Department)
            .Where(a => a.PatientId == patientId);

        if (request.UpcomingOnly)
        {
            var now = _clock.UtcNow;
            query = query.Where(a => a.ScheduledStart >= now
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && a.Status != AppointmentStatus.Completed);
        }
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AppointmentStatus>(request.Status, ignoreCase: true, out var s))
        {
            query = query.Where(a => a.Status == s);
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var rows = await query
            .OrderByDescending(a => a.ScheduledStart)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedList<PortalAppointmentSummaryResponse>(
            rows.Select(PortalMapping.ToAppointmentSummary).ToList(),
            totalCount, request.PageNumber, request.PageSize);
    }
}

public sealed record GetPortalAppointmentQuery(Guid Id) : IRequest<Result<PortalAppointmentDetailResponse>>;

public sealed class GetPortalAppointmentQueryHandler
    : IRequestHandler<GetPortalAppointmentQuery, Result<PortalAppointmentDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetPortalAppointmentQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PortalAppointmentDetailResponse>> Handle(GetPortalAppointmentQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var appointment = await _db.Appointments.AsNoTracking()
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .Include(a => a.Department)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);

        if (appointment is null || appointment.Patient?.UserId != userId)
        {
            return PortalErrors.AppointmentNotFound;
        }

        return PortalMapping.ToAppointmentDetail(appointment);
    }
}

// ==================== Appointments — request ====================
public sealed record RequestAppointmentCommand(
    Guid DoctorId, Guid? DepartmentId,
    DateTimeOffset ScheduledStart, int DurationMinutes,
    string? Reason) : IRequest<Result<PortalAppointmentDetailResponse>>;

public sealed class RequestAppointmentCommandValidator : AbstractValidator<RequestAppointmentCommand>
{
    public RequestAppointmentCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.DurationMinutes).InclusiveBetween(5, 240);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class RequestAppointmentCommandHandler
    : IRequestHandler<RequestAppointmentCommand, Result<PortalAppointmentDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IAppointmentNumberGenerator _numberGenerator;
    private readonly IDateTimeProvider _clock;
    private readonly ISender _mediator;

    public RequestAppointmentCommandHandler(
        IApplicationDbContext db,
        ITenantContext tenantContext,
        ICurrentUserService currentUser,
        IAppointmentNumberGenerator numberGenerator,
        IDateTimeProvider clock,
        ISender mediator)
    {
        _db = db;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _numberGenerator = numberGenerator;
        _clock = clock;
        _mediator = mediator;
    }

    public async Task<Result<PortalAppointmentDetailResponse>> Handle(RequestAppointmentCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure) return Result<PortalAppointmentDetailResponse>.Failure(tenantIdResult.Error);
        var tenantId = tenantIdResult.Value;

        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (patient is null) return PortalErrors.NoPatientRecord;
        if (!patient.IsActive) return AppointmentErrors.PatientInactive;

        var scheduledEnd = request.ScheduledStart.AddMinutes(request.DurationMinutes);
        if (scheduledEnd <= request.ScheduledStart) return AppointmentErrors.InvalidTime;
        if (request.ScheduledStart < _clock.UtcNow.AddMinutes(-5)) return AppointmentErrors.InPast;

        var doctor = await _db.Doctors.Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken).ConfigureAwait(false);
        if (doctor is null) return AppointmentErrors.DoctorNotFound;
        if (!doctor.IsActive) return AppointmentErrors.DoctorInactive;

        Guid? departmentId = request.DepartmentId ?? doctor.DepartmentId;
        if (departmentId.HasValue)
        {
            var deptExists = await _db.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken).ConfigureAwait(false);
            if (!deptExists) return AppointmentErrors.DepartmentNotFound;
        }

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .Where(t => t.Id == tenantId)
            .Select(t => new { t.Currency, t.DefaultConsultationFee })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var effectiveFee = doctor.ConsultationFee ?? doctor.Department?.DefaultConsultationFee ?? tenant?.DefaultConsultationFee;

        var appointmentNumber = await _numberGenerator.GenerateAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AppointmentNumber = appointmentNumber,
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            DepartmentId = departmentId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = scheduledEnd,
            Status = AppointmentStatus.Requested,
            Reason = request.Reason?.Trim(),
            ConsultationFee = effectiveFee,
            Currency = tenant?.Currency ?? "USD"
        };

        await _db.Appointments.AddAsync(appointment, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetPortalAppointmentQuery(appointment.Id), cancellationToken).ConfigureAwait(false);
    }
}

// ==================== Appointments — cancel own ====================
public sealed record CancelPortalAppointmentCommand(Guid Id, string Reason) : IRequest<Result>;

public sealed class CancelPortalAppointmentCommandValidator : AbstractValidator<CancelPortalAppointmentCommand>
{
    public CancelPortalAppointmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CancelPortalAppointmentCommandHandler : IRequestHandler<CancelPortalAppointmentCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public CancelPortalAppointmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(CancelPortalAppointmentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken).ConfigureAwait(false);

        if (appointment is null || appointment.Patient?.UserId != userId)
        {
            return PortalErrors.AppointmentNotFound;
        }

        // Patient can only cancel their own requests, bookings, or confirmed appointments.
        if (appointment.Status is not (AppointmentStatus.Requested or AppointmentStatus.Booked or AppointmentStatus.Confirmed))
        {
            return PortalErrors.CannotCancel;
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = _clock.UtcNow;
        appointment.CancelledByUserId = userId;
        appointment.CancellationReason = request.Reason.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// ==================== Prescriptions ====================
public sealed record ListPortalPrescriptionsQuery(int PageNumber, int PageSize)
    : IRequest<Result<PaginatedList<PortalPrescriptionResponse>>>;

public sealed class ListPortalPrescriptionsQueryHandler
    : IRequestHandler<ListPortalPrescriptionsQuery, Result<PaginatedList<PortalPrescriptionResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ListPortalPrescriptionsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<PortalPrescriptionResponse>>> Handle(
        ListPortalPrescriptionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var patientId = await _db.Patients.AsNoTracking()
            .Where(p => p.UserId == userId).Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (patientId is null) return PortalErrors.NoPatientRecord;

        var query = _db.Prescriptions.AsNoTracking()
            .Include(p => p.Items)
            .Include(p => p.Visit).ThenInclude(v => v!.Doctor).ThenInclude(d => d!.User)
            .Where(p => p.Visit != null && p.Visit.PatientId == patientId);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var rows = await query
            .OrderByDescending(p => p.IssuedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedList<PortalPrescriptionResponse>(
            rows.Select(PortalMapping.ToPrescription).ToList(),
            totalCount, request.PageNumber, request.PageSize);
    }
}

public sealed record GetPortalPrescriptionQuery(Guid Id) : IRequest<Result<PortalPrescriptionResponse>>;

public sealed class GetPortalPrescriptionQueryHandler : IRequestHandler<GetPortalPrescriptionQuery, Result<PortalPrescriptionResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetPortalPrescriptionQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PortalPrescriptionResponse>> Handle(GetPortalPrescriptionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var prescription = await _db.Prescriptions.AsNoTracking()
            .Include(p => p.Items)
            .Include(p => p.Visit).ThenInclude(v => v!.Doctor).ThenInclude(d => d!.User)
            .Include(p => p.Visit).ThenInclude(v => v!.Patient)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken).ConfigureAwait(false);

        if (prescription?.Visit?.Patient?.UserId != userId)
        {
            return PortalErrors.PrescriptionNotFound;
        }

        return PortalMapping.ToPrescription(prescription);
    }
}

// ==================== Invoices ====================
public sealed record ListPortalInvoicesQuery(int PageNumber, int PageSize, string? Status)
    : IRequest<Result<PaginatedList<PortalInvoiceSummaryResponse>>>;

public sealed class ListPortalInvoicesQueryHandler
    : IRequestHandler<ListPortalInvoicesQuery, Result<PaginatedList<PortalInvoiceSummaryResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ListPortalInvoicesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<PortalInvoiceSummaryResponse>>> Handle(
        ListPortalInvoicesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var patientId = await _db.Patients.AsNoTracking()
            .Where(p => p.UserId == userId).Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (patientId is null) return PortalErrors.NoPatientRecord;

        // Patients don't see draft invoices — those are still being prepared by staff.
        var query = _db.Invoices.AsNoTracking()
            .Include(i => i.Appointment)
            .Where(i => i.PatientId == patientId && i.Status != InvoiceStatus.Draft);

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<InvoiceStatus>(request.Status, ignoreCase: true, out var s))
        {
            query = query.Where(i => i.Status == s);
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var rows = await query
            .OrderByDescending(i => i.IssuedAt ?? i.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PaginatedList<PortalInvoiceSummaryResponse>(
            rows.Select(PortalMapping.ToInvoiceSummary).ToList(),
            totalCount, request.PageNumber, request.PageSize);
    }
}

public sealed record GetPortalInvoiceQuery(Guid Id) : IRequest<Result<PortalInvoiceDetailResponse>>;

public sealed class GetPortalInvoiceQueryHandler : IRequestHandler<GetPortalInvoiceQuery, Result<PortalInvoiceDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetPortalInvoiceQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<PortalInvoiceDetailResponse>> Handle(GetPortalInvoiceQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var invoice = await _db.Invoices.AsNoTracking()
            .Include(i => i.Appointment)
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .Include(i => i.Patient)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken).ConfigureAwait(false);

        if (invoice is null || invoice.Status == InvoiceStatus.Draft || invoice.Patient?.UserId != userId)
        {
            return PortalErrors.InvoiceNotFound;
        }

        return PortalMapping.ToInvoiceDetail(invoice);
    }
}

// ==================== Dashboard ====================
public sealed record GetPortalDashboardQuery : IRequest<Result<PortalDashboardResponse>>;

public sealed class GetPortalDashboardQueryHandler : IRequestHandler<GetPortalDashboardQuery, Result<PortalDashboardResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public GetPortalDashboardQueryHandler(
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

    public async Task<Result<PortalDashboardResponse>> Handle(GetPortalDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return PortalErrors.NoPatientRecord;

        var patient = await _db.Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (patient is null) return PortalErrors.NoPatientRecord;

        var now = _clock.UtcNow;

        var upcomingQuery = _db.Appointments.AsNoTracking()
            .Include(a => a.Doctor).ThenInclude(d => d!.User)
            .Include(a => a.Department)
            .Where(a => a.PatientId == patient.Id
                && a.ScheduledStart >= now
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && a.Status != AppointmentStatus.Completed);

        var upcomingCount = await upcomingQuery.CountAsync(cancellationToken).ConfigureAwait(false);
        var nextAppointment = await upcomingQuery
            .OrderBy(a => a.ScheduledStart)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var pendingRequestCount = await _db.Appointments.AsNoTracking()
            .Where(a => a.PatientId == patient.Id && a.Status == AppointmentStatus.Requested)
            .CountAsync(cancellationToken).ConfigureAwait(false);

        var prescriptionCount = await _db.Prescriptions.AsNoTracking()
            .Where(p => p.Visit != null && p.Visit.PatientId == patient.Id)
            .CountAsync(cancellationToken).ConfigureAwait(false);

        var outstandingInvoices = await _db.Invoices.AsNoTracking()
            .Where(i => i.PatientId == patient.Id
                && (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid))
            .Select(i => new { i.Total, i.AmountPaid, i.Currency })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var outstandingCount = outstandingInvoices.Count;
        var outstandingAmount = outstandingInvoices.Sum(i => i.Total - i.AmountPaid);

        string currency = outstandingInvoices.FirstOrDefault()?.Currency ?? string.Empty;
        if (string.IsNullOrEmpty(currency))
        {
            var tenantIdResult = _tenantContext.RequireTenantId();
            if (tenantIdResult.IsSuccess)
            {
                currency = await _db.Tenants.IgnoreQueryFilters()
                    .Where(t => t.Id == tenantIdResult.Value)
                    .Select(t => t.Currency)
                    .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "USD";
            }
            else
            {
                currency = "USD";
            }
        }

        return new PortalDashboardResponse(
            nextAppointment is null ? null : PortalMapping.ToAppointmentSummary(nextAppointment),
            upcomingCount,
            pendingRequestCount,
            prescriptionCount,
            outstandingCount,
            outstandingAmount,
            currency);
    }
}
