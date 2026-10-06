using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Patients;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Patients.MedicalHistory;

// -------------------- List --------------------
public sealed record ListMedicalHistoryQuery(Guid PatientId) : IRequest<Result<IReadOnlyList<MedicalHistoryEntryResponse>>>;

public sealed class ListMedicalHistoryQueryHandler : IRequestHandler<ListMedicalHistoryQuery, Result<IReadOnlyList<MedicalHistoryEntryResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListMedicalHistoryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<MedicalHistoryEntryResponse>>> Handle(ListMedicalHistoryQuery request, CancellationToken cancellationToken)
    {
        var patientExists = await _db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken).ConfigureAwait(false);
        if (!patientExists)
        {
            return PatientErrors.NotFound;
        }

        var entries = await _db.MedicalHistoryEntries.AsNoTracking()
            .Where(m => m.PatientId == request.PatientId)
            .OrderByDescending(m => m.Since ?? DateOnly.MinValue)
            .Select(m => new MedicalHistoryEntryResponse(
                m.Id, m.PatientId, m.Condition, m.Since, m.Notes, m.IsChronic, m.IsResolved, m.CreatedOn))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return entries;
    }
}

// -------------------- Add --------------------
public sealed record AddMedicalHistoryCommand(
    Guid PatientId, string Condition, DateOnly? Since, string? Notes, bool IsChronic, bool IsResolved)
    : IRequest<Result<MedicalHistoryEntryResponse>>;

public sealed class AddMedicalHistoryCommandValidator : AbstractValidator<AddMedicalHistoryCommand>
{
    public AddMedicalHistoryCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Condition).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class AddMedicalHistoryCommandHandler : IRequestHandler<AddMedicalHistoryCommand, Result<MedicalHistoryEntryResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public AddMedicalHistoryCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MedicalHistoryEntryResponse>> Handle(AddMedicalHistoryCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<MedicalHistoryEntryResponse>.Failure(tenantIdResult.Error);
        }

        var patientExists = await _db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken).ConfigureAwait(false);
        if (!patientExists)
        {
            return PatientErrors.NotFound;
        }

        var entry = new MedicalHistoryEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            PatientId = request.PatientId,
            Condition = request.Condition.Trim(),
            Since = request.Since,
            Notes = request.Notes?.Trim(),
            IsChronic = request.IsChronic,
            IsResolved = request.IsResolved
        };

        await _db.MedicalHistoryEntries.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new MedicalHistoryEntryResponse(
            entry.Id, entry.PatientId, entry.Condition, entry.Since, entry.Notes,
            entry.IsChronic, entry.IsResolved, entry.CreatedOn);
    }
}

// -------------------- Update --------------------
public sealed record UpdateMedicalHistoryCommand(
    Guid Id, string Condition, DateOnly? Since, string? Notes, bool IsChronic, bool IsResolved)
    : IRequest<Result<MedicalHistoryEntryResponse>>;

public sealed class UpdateMedicalHistoryCommandValidator : AbstractValidator<UpdateMedicalHistoryCommand>
{
    public UpdateMedicalHistoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Condition).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class UpdateMedicalHistoryCommandHandler : IRequestHandler<UpdateMedicalHistoryCommand, Result<MedicalHistoryEntryResponse>>
{
    private readonly IApplicationDbContext _db;

    public UpdateMedicalHistoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<MedicalHistoryEntryResponse>> Handle(UpdateMedicalHistoryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _db.MedicalHistoryEntries.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (entry is null)
        {
            return PatientErrors.MedicalHistory.NotFound;
        }

        entry.Condition = request.Condition.Trim();
        entry.Since = request.Since;
        entry.Notes = request.Notes?.Trim();
        entry.IsChronic = request.IsChronic;
        entry.IsResolved = request.IsResolved;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new MedicalHistoryEntryResponse(
            entry.Id, entry.PatientId, entry.Condition, entry.Since, entry.Notes,
            entry.IsChronic, entry.IsResolved, entry.CreatedOn);
    }
}

// -------------------- Delete --------------------
public sealed record DeleteMedicalHistoryCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteMedicalHistoryCommandHandler : IRequestHandler<DeleteMedicalHistoryCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteMedicalHistoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteMedicalHistoryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _db.MedicalHistoryEntries.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (entry is null)
        {
            return PatientErrors.MedicalHistory.NotFound;
        }
        _db.MedicalHistoryEntries.Remove(entry);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
