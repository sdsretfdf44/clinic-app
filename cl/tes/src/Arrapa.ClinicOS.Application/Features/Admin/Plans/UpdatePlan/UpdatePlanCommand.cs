using System.Text.Json;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Admin.Plans;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Plans.UpdatePlan;

public sealed record UpdatePlanCommand(
    Guid PlanId,
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

public sealed class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64).Matches("^[a-z0-9_-]+$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Interval).Must(v => Enum.TryParse<SubscriptionInterval>(v, ignoreCase: true, out _));
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Currency).NotEmpty().Length(3, 10);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, Result<SubscriptionPlanResponse>>
{
    private readonly IApplicationDbContext _db;

    public UpdatePlanCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<SubscriptionPlanResponse>> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _db.SubscriptionPlans.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && !p.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (plan is null)
        {
            return AdminErrors.Plans.NotFound;
        }

        var code = request.Code.Trim().ToLowerInvariant();
        if (!string.Equals(code, plan.Code, StringComparison.Ordinal))
        {
            var duplicate = await _db.SubscriptionPlans.IgnoreQueryFilters()
                .AnyAsync(p => p.Id != plan.Id && p.Code == code, cancellationToken).ConfigureAwait(false);
            if (duplicate)
            {
                return AdminErrors.Plans.CodeAlreadyExists;
            }
        }

        var tenantCount = await _db.Tenants.IgnoreQueryFilters()
            .CountAsync(t => t.SubscriptionPlanId == plan.Id && !t.IsDeleted, cancellationToken).ConfigureAwait(false);

        if (plan.IsActive && !request.IsActive && tenantCount > 0)
        {
            return AdminErrors.Plans.CannotDeactivateInUse;
        }

        if (request.IsDefault && !plan.IsDefault)
        {
            var currentDefaults = await _db.SubscriptionPlans.IgnoreQueryFilters()
                .Where(p => p.IsDefault && p.Id != plan.Id && !p.IsDeleted)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            foreach (var d in currentDefaults)
            {
                d.IsDefault = false;
            }
        }

        plan.Code = code;
        plan.Name = request.Name.Trim();
        plan.Description = request.Description?.Trim();
        plan.Interval = Enum.Parse<SubscriptionInterval>(request.Interval, ignoreCase: true);
        plan.Price = request.Price;
        plan.Currency = request.Currency.Trim().ToUpperInvariant();
        plan.MaxUsers = request.MaxUsers;
        plan.MaxPatients = request.MaxPatients;
        plan.MaxAppointmentsPerMonth = request.MaxAppointmentsPerMonth;
        plan.FeaturesJson = JsonSerializer.Serialize(request.Features ?? Array.Empty<string>());
        plan.IsActive = request.IsActive;
        plan.IsDefault = request.IsDefault;
        plan.SortOrder = request.SortOrder;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new SubscriptionPlanResponse(
            plan.Id, plan.Code, plan.Name, plan.Description, plan.Interval.ToString(), plan.Price, plan.Currency,
            plan.MaxUsers, plan.MaxPatients, plan.MaxAppointmentsPerMonth, request.Features ?? Array.Empty<string>(),
            plan.IsActive, plan.IsDefault, plan.SortOrder, tenantCount);
    }
}
