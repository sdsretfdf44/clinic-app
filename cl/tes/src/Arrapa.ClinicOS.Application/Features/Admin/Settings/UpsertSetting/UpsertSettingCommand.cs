using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.Settings;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Settings;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Settings.UpsertSetting;

public sealed record UpsertSettingCommand(
    string Key,
    string Value,
    string Category,
    string DataType,
    string? Description,
    bool IsSecret) : IRequest<Result<GlobalSettingResponse>>;

public sealed class UpsertSettingCommandValidator : AbstractValidator<UpsertSettingCommand>
{
    public UpsertSettingCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(128).Matches("^[a-z][a-z0-9_.]*$").WithMessage("Key must start with a lowercase letter and contain only lowercase letters, digits, underscores, or periods.");
        RuleFor(x => x.Value).NotNull().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(64);
        RuleFor(x => x.DataType).Must(v => Enum.TryParse<SettingDataType>(v, ignoreCase: true, out _)).WithMessage("Data type must be one of: String, Integer, Decimal, Boolean, Json.");
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpsertSettingCommandHandler : IRequestHandler<UpsertSettingCommand, Result<GlobalSettingResponse>>
{
    private readonly IApplicationDbContext _db;

    public UpsertSettingCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<GlobalSettingResponse>> Handle(UpsertSettingCommand request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim().ToLowerInvariant();
        var dataType = Enum.Parse<SettingDataType>(request.DataType, ignoreCase: true);

        var existing = await _db.GlobalSettings.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted, cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            existing = new GlobalSetting
            {
                Id = Guid.NewGuid(),
                TenantId = AppConstants.SystemTenants.PlatformTenantId,
                Key = key
            };
            await _db.GlobalSettings.AddAsync(existing, cancellationToken).ConfigureAwait(false);
        }

        existing.Value = request.Value;
        existing.Category = request.Category.Trim();
        existing.DataType = dataType;
        existing.Description = request.Description?.Trim();
        existing.IsSecret = request.IsSecret;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new GlobalSettingResponse(
            existing.Id, existing.Key,
            existing.IsSecret ? "***" : existing.Value,
            existing.Category, existing.DataType.ToString(), existing.Description, existing.IsSecret, existing.UpdatedOn);
    }
}
