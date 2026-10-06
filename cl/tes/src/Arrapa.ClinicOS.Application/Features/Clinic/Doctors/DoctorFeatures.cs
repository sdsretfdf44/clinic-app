using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Clinic.Common;
using Arrapa.ClinicOS.Contracts.Clinic.Doctors;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Doctors;

// -------------------- List --------------------
public sealed record ListDoctorsQuery(
    int PageNumber, int PageSize, string? Search, Guid? DepartmentId, bool? IsActive) : IRequest<Result<PaginatedList<DoctorSummaryResponse>>>;

public sealed class ListDoctorsQueryHandler : IRequestHandler<ListDoctorsQuery, Result<PaginatedList<DoctorSummaryResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ListDoctorsQueryHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PaginatedList<DoctorSummaryResponse>>> Handle(ListDoctorsQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<PaginatedList<DoctorSummaryResponse>>.Failure(tenantIdResult.Error);
        }

        var query = _db.Doctors.AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == request.DepartmentId);
        }
        if (request.IsActive.HasValue)
        {
            query = query.Where(d => d.IsActive == request.IsActive);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(d =>
                d.User!.FirstName.ToLower().Contains(term) ||
                d.User!.LastName.ToLower().Contains(term) ||
                d.User!.Email!.ToLower().Contains(term) ||
                (d.Specialty != null && d.Specialty.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var tenantCurrency = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantIdResult.Value).Select(t => t.Currency)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "USD";

        var tenantDefaultFee = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantIdResult.Value).Select(t => t.DefaultConsultationFee)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var rows = await query
            .OrderBy(d => d.SortOrder).ThenBy(d => d.User!.LastName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new
            {
                d.Id,
                d.UserId,
                d.User!.FirstName,
                d.User!.LastName,
                Email = d.User!.Email ?? string.Empty,
                d.User!.PhoneNumber,
                d.User!.AvatarUrl,
                d.Specialty,
                d.DepartmentId,
                DepartmentName = d.Department != null ? d.Department.Name : null,
                DepartmentFee = d.Department != null ? d.Department.DefaultConsultationFee : null,
                d.ConsultationFee,
                d.IsActive,
                d.IsAcceptingNewPatients,
                d.CreatedOn
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var items = rows.Select(r => new DoctorSummaryResponse(
            r.Id, r.UserId,
            $"{r.FirstName} {r.LastName}".Trim(), r.Email, r.PhoneNumber, r.AvatarUrl,
            r.Specialty, r.DepartmentId, r.DepartmentName,
            r.ConsultationFee ?? r.DepartmentFee ?? tenantDefaultFee,
            tenantCurrency, r.IsActive, r.IsAcceptingNewPatients, r.CreatedOn)).ToList();

        return new PaginatedList<DoctorSummaryResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

// -------------------- Get --------------------
public sealed record GetDoctorQuery(Guid Id) : IRequest<Result<DoctorDetailResponse>>;

public sealed class GetDoctorQueryHandler : IRequestHandler<GetDoctorQuery, Result<DoctorDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetDoctorQueryHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<DoctorDetailResponse>> Handle(GetDoctorQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<DoctorDetailResponse>.Failure(tenantIdResult.Error);
        }

        var doctor = await _db.Doctors.AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (doctor?.User is null)
        {
            return ClinicErrors.Doctors.NotFound;
        }

        var tenant = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantIdResult.Value)
            .Select(t => new { t.Currency, t.DefaultConsultationFee })
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var effectiveFee = doctor.ConsultationFee ?? doctor.Department?.DefaultConsultationFee ?? tenant?.DefaultConsultationFee;

        return new DoctorDetailResponse(
            doctor.Id, doctor.UserId,
            doctor.User.FirstName, doctor.User.LastName, doctor.User.FullName,
            doctor.User.Email ?? string.Empty, doctor.User.PhoneNumber, doctor.User.AvatarUrl,
            doctor.Specialty, doctor.LicenseNumber, doctor.RegistrationNumber,
            doctor.DepartmentId, doctor.Department?.Name,
            doctor.ConsultationFee, effectiveFee, tenant?.Currency ?? "USD",
            doctor.Bio, doctor.IsAcceptingNewPatients, doctor.IsActive, doctor.SortOrder,
            doctor.CreatedOn, doctor.UpdatedOn);
    }
}

// -------------------- Create --------------------
public sealed record CreateDoctorCommand(
    string FirstName, string LastName, string Email, string? PhoneNumber,
    string? Specialty, string? LicenseNumber, string? RegistrationNumber,
    Guid? DepartmentId, decimal? ConsultationFee, string? Bio, bool IsAcceptingNewPatients)
    : IRequest<Result<DoctorDetailResponse>>;

public sealed class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.Specialty).MaximumLength(200);
        RuleFor(x => x.LicenseNumber).MaximumLength(64);
        RuleFor(x => x.RegistrationNumber).MaximumLength(64);
        RuleFor(x => x.Bio).MaximumLength(1000);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0m).When(x => x.ConsultationFee.HasValue);
    }
}

public sealed class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Result<DoctorDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ClinicUserProvisioning _provisioning;
    private readonly ISender _mediator;

    public CreateDoctorCommandHandler(
        IApplicationDbContext db,
        ITenantContext tenantContext,
        ClinicUserProvisioning provisioning,
        ISender mediator)
    {
        _db = db;
        _tenantContext = tenantContext;
        _provisioning = provisioning;
        _mediator = mediator;
    }

    public async Task<Result<DoctorDetailResponse>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<DoctorDetailResponse>.Failure(tenantIdResult.Error);
        }
        var tenantId = tenantIdResult.Value;

        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId, cancellationToken).ConfigureAwait(false);
            if (!deptExists)
            {
                return ClinicErrors.Departments.NotFound;
            }
        }

        var userResult = await _provisioning.ProvisionAsync(
            tenantId, request.FirstName, request.LastName, request.Email, request.PhoneNumber,
            UserRole.Doctor, AppConstants.Roles.Doctor, ClinicErrors.Doctors.EmailInUse, cancellationToken)
            .ConfigureAwait(false);

        if (userResult.IsFailure)
        {
            return Result<DoctorDetailResponse>.Failure(userResult.Error);
        }

        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userResult.Value.Id,
            Specialty = request.Specialty?.Trim(),
            LicenseNumber = request.LicenseNumber?.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            DepartmentId = request.DepartmentId,
            ConsultationFee = request.ConsultationFee,
            Bio = request.Bio?.Trim(),
            IsAcceptingNewPatients = request.IsAcceptingNewPatients,
            IsActive = true
        };

        await _db.Doctors.AddAsync(doctor, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetDoctorQuery(doctor.Id), cancellationToken).ConfigureAwait(false);
    }
}

// -------------------- Update --------------------
public sealed record UpdateDoctorCommand(
    Guid Id,
    string FirstName, string LastName, string? PhoneNumber, string? AvatarUrl,
    string? Specialty, string? LicenseNumber, string? RegistrationNumber,
    Guid? DepartmentId, decimal? ConsultationFee, string? Bio,
    bool IsAcceptingNewPatients, int SortOrder) : IRequest<Result<DoctorDetailResponse>>;

public sealed class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.AvatarUrl).MaximumLength(500);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0m).When(x => x.ConsultationFee.HasValue);
    }
}

public sealed class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, Result<DoctorDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<Arrapa.ClinicOS.Domain.Identity.ApplicationUser> _userManager;
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _clock;

    public UpdateDoctorCommandHandler(
        IApplicationDbContext db,
        UserManager<Arrapa.ClinicOS.Domain.Identity.ApplicationUser> userManager,
        ISender mediator,
        IDateTimeProvider clock)
    {
        _db = db;
        _userManager = userManager;
        _mediator = mediator;
        _clock = clock;
    }

    public async Task<Result<DoctorDetailResponse>> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (doctor is null || doctor.User is null)
        {
            return ClinicErrors.Doctors.NotFound;
        }

        if (request.DepartmentId.HasValue)
        {
            var deptExists = await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId, cancellationToken).ConfigureAwait(false);
            if (!deptExists)
            {
                return ClinicErrors.Departments.NotFound;
            }
        }

        doctor.User.FirstName = request.FirstName.Trim();
        doctor.User.LastName = request.LastName.Trim();
        doctor.User.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        doctor.User.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        await _userManager.UpdateAsync(doctor.User).ConfigureAwait(false);

        doctor.Specialty = request.Specialty?.Trim();
        doctor.LicenseNumber = request.LicenseNumber?.Trim();
        doctor.RegistrationNumber = request.RegistrationNumber?.Trim();
        doctor.DepartmentId = request.DepartmentId;
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.Bio = request.Bio?.Trim();
        doctor.IsAcceptingNewPatients = request.IsAcceptingNewPatients;
        doctor.SortOrder = request.SortOrder;
        doctor.UpdatedOn = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await _mediator.Send(new GetDoctorQuery(doctor.Id), cancellationToken).ConfigureAwait(false);
    }
}

// -------------------- Toggle active --------------------
public sealed record SetDoctorActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;

public sealed class SetDoctorActiveCommandHandler : IRequestHandler<SetDoctorActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<Arrapa.ClinicOS.Domain.Identity.ApplicationUser> _userManager;
    private readonly IDateTimeProvider _clock;

    public SetDoctorActiveCommandHandler(
        IApplicationDbContext db,
        UserManager<Arrapa.ClinicOS.Domain.Identity.ApplicationUser> userManager,
        IDateTimeProvider clock)
    {
        _db = db;
        _userManager = userManager;
        _clock = clock;
    }

    public async Task<Result> Handle(SetDoctorActiveCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (doctor is null || doctor.User is null)
        {
            return ClinicErrors.Doctors.NotFound;
        }

        doctor.IsActive = request.IsActive;
        doctor.User.IsActive = request.IsActive;
        await _userManager.UpdateAsync(doctor.User).ConfigureAwait(false);

        if (!request.IsActive)
        {
            var tokens = await _db.RefreshTokens.IgnoreQueryFilters()
                .Where(t => t.UserId == doctor.UserId && t.RevokedAt == null)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var now = _clock.UtcNow;
            foreach (var t in tokens)
            {
                t.RevokedAt = now;
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// -------------------- Delete --------------------
public sealed record DeleteDoctorCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteDoctorCommandHandler : IRequestHandler<DeleteDoctorCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<Arrapa.ClinicOS.Domain.Identity.ApplicationUser> _userManager;

    public DeleteDoctorCommandHandler(
        IApplicationDbContext db,
        UserManager<Arrapa.ClinicOS.Domain.Identity.ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<Result> Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (doctor is null)
        {
            return ClinicErrors.Doctors.NotFound;
        }

        _db.Doctors.Remove(doctor);
        if (doctor.User is not null)
        {
            doctor.User.IsActive = false;
            await _userManager.UpdateAsync(doctor.User).ConfigureAwait(false);
        }
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
