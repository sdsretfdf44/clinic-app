using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Clinic.Departments;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Departments;

// -------------------- List --------------------
public sealed record ListDepartmentsQuery(bool IncludeInactive) : IRequest<Result<IReadOnlyList<DepartmentResponse>>>;

public sealed class ListDepartmentsQueryHandler : IRequestHandler<ListDepartmentsQuery, Result<IReadOnlyList<DepartmentResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListDepartmentsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<DepartmentResponse>>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Departments.AsNoTracking();
        if (!request.IncludeInactive)
        {
            query = query.Where(d => d.IsActive);
        }

        var departments = await query
            .OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
            .Select(d => new DepartmentResponse(
                d.Id, d.Name, d.Description, d.DefaultConsultationFee,
                d.IsActive, d.SortOrder,
                _db.Doctors.Count(doc => doc.DepartmentId == d.Id),
                d.CreatedOn))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return departments;
    }
}

// -------------------- Get --------------------
public sealed record GetDepartmentQuery(Guid Id) : IRequest<Result<DepartmentResponse>>;

public sealed class GetDepartmentQueryHandler : IRequestHandler<GetDepartmentQuery, Result<DepartmentResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetDepartmentQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<DepartmentResponse>> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
    {
        var department = await _db.Departments.AsNoTracking()
            .Where(d => d.Id == request.Id)
            .Select(d => new DepartmentResponse(
                d.Id, d.Name, d.Description, d.DefaultConsultationFee,
                d.IsActive, d.SortOrder,
                _db.Doctors.Count(doc => doc.DepartmentId == d.Id),
                d.CreatedOn))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return department is null ? ClinicErrors.Departments.NotFound : department;
    }
}

// -------------------- Create --------------------
public sealed record CreateDepartmentCommand(
    string Name,
    string? Description,
    decimal? DefaultConsultationFee,
    bool IsActive,
    int SortOrder) : IRequest<Result<DepartmentResponse>>;

public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DefaultConsultationFee).GreaterThanOrEqualTo(0m).When(x => x.DefaultConsultationFee.HasValue);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CreateDepartmentCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<DepartmentResponse>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<DepartmentResponse>.Failure(tenantIdResult.Error);
        }

        var name = request.Name.Trim();
        var duplicate = await _db.Departments.AnyAsync(d => d.Name == name, cancellationToken).ConfigureAwait(false);
        if (duplicate)
        {
            return ClinicErrors.Departments.NameExists;
        }

        var department = new Department
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            Name = name,
            Description = request.Description?.Trim(),
            DefaultConsultationFee = request.DefaultConsultationFee,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        };

        await _db.Departments.AddAsync(department, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new DepartmentResponse(
            department.Id, department.Name, department.Description, department.DefaultConsultationFee,
            department.IsActive, department.SortOrder, DoctorCount: 0, department.CreatedOn);
    }
}

// -------------------- Update --------------------
public sealed record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal? DefaultConsultationFee,
    bool IsActive,
    int SortOrder) : IRequest<Result<DepartmentResponse>>;

public sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DefaultConsultationFee).GreaterThanOrEqualTo(0m).When(x => x.DefaultConsultationFee.HasValue);
    }
}

public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result<DepartmentResponse>>
{
    private readonly IApplicationDbContext _db;

    public UpdateDepartmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<DepartmentResponse>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _db.Departments.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (department is null)
        {
            return ClinicErrors.Departments.NotFound;
        }

        var name = request.Name.Trim();
        if (!string.Equals(name, department.Name, StringComparison.Ordinal))
        {
            var duplicate = await _db.Departments.AnyAsync(d => d.Id != department.Id && d.Name == name, cancellationToken).ConfigureAwait(false);
            if (duplicate)
            {
                return ClinicErrors.Departments.NameExists;
            }
        }

        department.Name = name;
        department.Description = request.Description?.Trim();
        department.DefaultConsultationFee = request.DefaultConsultationFee;
        department.IsActive = request.IsActive;
        department.SortOrder = request.SortOrder;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var doctorCount = await _db.Doctors.CountAsync(doc => doc.DepartmentId == department.Id, cancellationToken).ConfigureAwait(false);
        return new DepartmentResponse(
            department.Id, department.Name, department.Description, department.DefaultConsultationFee,
            department.IsActive, department.SortOrder, doctorCount, department.CreatedOn);
    }
}

// -------------------- Delete --------------------
public sealed record DeleteDepartmentCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteDepartmentCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _db.Departments.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (department is null)
        {
            return ClinicErrors.Departments.NotFound;
        }

        var hasDoctors = await _db.Doctors.AnyAsync(doc => doc.DepartmentId == department.Id, cancellationToken).ConfigureAwait(false);
        if (hasDoctors)
        {
            return ClinicErrors.Departments.HasDoctors;
        }

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
