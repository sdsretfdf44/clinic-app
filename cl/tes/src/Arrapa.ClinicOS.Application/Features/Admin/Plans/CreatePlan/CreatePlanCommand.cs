using System.Text.Json;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Admin.Plans;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Subscriptions;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Plans.CreatePlan;

public sealed record CreatePlanCommand(
    string Code,
    string Name,
    string? Description,
    string Interval,
    decimal Price,
    string Currency,
    int? MaxUsers,
    int? MaxPatients,
    int? MaxAppointmentsPerMonth,
    IReadOnlyList<string> Features,
    bool IsActive,
    bool IsDefault,
    int SortOrder) : IRequest<Result<SubscriptionPlanResponse>>;

public sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64).Matches("^[a-z0-9_-]+$").WithMessage("Code may only contain lowercase letters, digits, underscores, and hyphens.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Interval).Must(v => Enum.TryParse<SubscriptionInterval>(v, ignoreCase: true, out _)).WithMessage("Interval must be Monthly or Annual.");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Currency).NotEmpty().Length(3, 10);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Result<SubscriptionPlanResponse>>
{
    private readonly IApplicationDbContext _db;

    public CreatePlanCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<SubscriptionPlanResponse>> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToLowerInvariant();
        var exists = await _db.SubscriptionPlans.IgnoreQueryFilters().AnyAsync(p => p.Code == code, cancellationToken).ConfigureAwait(false);
        if (exists)
        {
            return AdminErrors.Plans.CodeAlreadyExists;
        }

        if (request.IsDefault)
        {
            await ClearOtherDefaultsAsync(cancellationToken).ConfigureAwait(false);
        }

        var interval = Enum.Parse<SubscriptionInterval>(request.Interval, ignoreCase: true);
        var featuresJson = JsonSerializer.Serialize(request.Features ?? Array.Empty<string>());

        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            TenantId = AppConstants.SystemTenants.PlatformTenantId,
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Interval = interval,
            Price = request.Price,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            MaxUsers = request.MaxUsers,
            MaxPatients = request.MaxPatients,
            MaxAppointmentsPerMonth = request.MaxAppointmentsPerMonth,
            FeaturesJson = featuresJson,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            SortOrder = request.SortOrder
        };

        await _db.SubscriptionPlans.AddAsync(plan, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new SubscriptionPlanResponse(
            plan.Id, plan.Code, plan.Name, plan.Description, plan.Interval.ToString(), plan.Price, plan.Currency,
            plan.MaxUsers, plan.MaxPatients, plan.MaxAppointmentsPerMonth, request.Features ?? Array.Empty<string>(),
            plan.IsActive, plan.IsDefault, plan.SortOrder, TenantCount: 0);
    }

    private async Task ClearOtherDefaultsAsync(CancellationToken cancellationToken)
    {
        var currentDefaults = await _db.SubscriptionPlans.IgnoreQueryFilters()
            .Where(p => p.IsDefault && !p.IsDeleted).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var plan in currentDefaults)
        {
            plan.IsDefault = false;
        }
    }
}
