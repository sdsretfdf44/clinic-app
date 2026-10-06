using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Shared.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.AssignPlan;

public sealed record AssignPlanCommand(Guid TenantId, Guid SubscriptionPlanId, DateTimeOffset? ExpiresAt) : IRequest<Result>;

public sealed class AssignPlanCommandValidator : AbstractValidator<AssignPlanCommand>
{
    public AssignPlanCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SubscriptionPlanId).NotEmpty();
    }
}

public sealed class AssignPlanCommandHandler : IRequestHandler<AssignPlanCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public AssignPlanCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(AssignPlanCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (tenant is null)
        {
            return AdminErrors.Tenants.NotFound;
        }

        var plan = await _db.SubscriptionPlans.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.SubscriptionPlanId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (plan is null)
        {
            return AdminErrors.Plans.NotFound;
        }

        tenant.SubscriptionPlanId = plan.Id;
        tenant.SubscriptionExpiresAt = request.ExpiresAt;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
