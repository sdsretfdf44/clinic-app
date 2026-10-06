using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string? ContactEmail,
    string? ContactPhone,
    string TimeZone,
    string Locale,
    string Currency,
    Guid? SubscriptionPlanId,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail) : IRequest<Result<TenantDetailResponse>>;
