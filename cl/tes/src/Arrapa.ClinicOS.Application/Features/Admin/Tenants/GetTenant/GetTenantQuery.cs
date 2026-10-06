using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.GetTenant;

public sealed record GetTenantQuery(Guid TenantId) : IRequest<Result<TenantDetailResponse>>;
