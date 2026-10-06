using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.ListTenants;

public sealed record ListTenantsQuery(
    int PageNumber,
    int PageSize,
    string? Search,
    string? Status,
    string? SortBy,
    SortDirection SortDirection) : IRequest<Result<PaginatedList<TenantSummaryResponse>>>;
