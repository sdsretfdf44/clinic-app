using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Clinic.Settings;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Settings;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Settings;

// -------------------- List --------------------
public sealed record ListClinicSettingsQuery(string? Category) : IRequest<Result<IReadOnlyList<ClinicSettingResponse>>>;

public sealed class ListClinicSettingsQueryHandler : IRequestHandler<ListClinicSettingsQuery, Result<IReadOnlyList<ClinicSettingResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListClinicSettingsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<ClinicSettingResponse>>> Handle(ListClinicSettingsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.TenantSettings.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(s => s.Category == request.Category);
        }

        var settings = await query
            .OrderBy(s => s.Category).ThenBy(s => s.Key)
            .Select(s => new ClinicSettingResponse(
                s.Id, s.Key,
                s.IsSecret ? "***" : s.Value,
                s.Category, s.DataType.ToString(), s.Description, s.IsSecret, s.UpdatedOn))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return settings;
    }
}

// -------------------- Upsert --------------------
public sealed record UpsertClinicSettingCommand(
    string Key,
    string Value,
    string Category,
    string DataType,
    string? Description,
    bool IsSecret) : IRequest<Result<ClinicSettingResponse>>;

public sealed class UpsertClinicSettingCommandValidator : AbstractValidator<UpsertClinicSettingCommand>
{
    public UpsertClinicSettingCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(128)
            .Matches("^[a-z][a-z0-9_.]*$")
            .WithMessage("Key must start with a lowercase letter and contain only lowercase letters, digits, underscores, or periods.");
        RuleFor(x => x.Value).NotNull().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(64);
        RuleFor(x => x.DataType).Must(v => Enum.TryParse<SettingDataType>(v, ignoreCase: true, out _))
            .WithMessage("Data type must be one of: String, Integer, Decimal, Boolean, Json.");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpsertClinicSettingCommandHandler : IRequestHandler<UpsertClinicSettingCommand, Result<ClinicSettingResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;

    public UpsertClinicSettingCommandHandler(IApplicationDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<ClinicSettingResponse>> Handle(UpsertClinicSettingCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<ClinicSettingResponse>.Failure(tenantIdResult.Error);
        }

        var key = request.Key.Trim().ToLowerInvariant();
        var dataType = Enum.Parse<SettingDataType>(request.DataType, ignoreCase: true);

        var existing = await _db.TenantSettings.FirstOrDefaultAsync(s => s.Key == key, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            existing = new TenantSetting
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdResult.Value,
                Key = key
            };
            await _db.TenantSettings.AddAsync(existing, cancellationToken).ConfigureAwait(false);
        }

        existing.Value = request.Value;
        existing.Category = request.Category.Trim();
        existing.DataType = dataType;
        existing.Description = request.Description?.Trim();
        existing.IsSecret = request.IsSecret;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ClinicSettingResponse(
            existing.Id, existing.Key,
            existing.IsSecret ? "***" : existing.Value,
            existing.Category, existing.DataType.ToString(), existing.Description, existing.IsSecret, existing.UpdatedOn);
    }
}
