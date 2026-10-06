using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.AuditLogs;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.AuditLogs.ListAuditLogs;

public sealed record ListAuditLogsQuery(
    int PageNumber,
    int PageSize,
    Guid? TenantId,
    string? EntityType,
    string? Operation,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Search) : IRequest<Result<PaginatedList<AuditLogResponse>>>;

public sealed class ListAuditLogsQueryHandler : IRequestHandler<ListAuditLogsQuery, Result<PaginatedList<AuditLogResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListAuditLogsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<PaginatedList<AuditLogResponse>>> Handle(ListAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.AuditLogs.AsNoTracking().IgnoreQueryFilters().Where(a => !a.IsDeleted);

        if (request.TenantId.HasValue && request.TenantId != Guid.Empty)
        {
            query = query.Where(a => a.TenantId == request.TenantId);
        }
        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }
        if (!string.IsNullOrWhiteSpace(request.Operation))
        {
            query = query.Where(a => a.Operation.ToString() == request.Operation);
        }
        if (request.From.HasValue)
        {
            query = query.Where(a => a.CreatedOn >= request.From);
        }
        if (request.To.HasValue)
        {
            query = query.Where(a => a.CreatedOn <= request.To);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(a =>
                (a.PerformedByEmail != null && a.PerformedByEmail.ToLower().Contains(term)) ||
                a.EntityId.Contains(term) ||
                (a.CorrelationId != null && a.CorrelationId.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var page = await query
            .OrderByDescending(a => a.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new
            {
                a.Id,
                a.TenantId,
                TenantName = _db.Tenants.IgnoreQueryFilters().Where(t => t.Id == a.TenantId).Select(t => t.Name).FirstOrDefault(),
                a.EntityType,
                a.EntityId,
                Operation = a.Operation.ToString(),
                a.PerformedByUserId,
                a.PerformedByEmail,
                a.IpAddress,
                a.CorrelationId,
                a.CreatedOn
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var items = page.Select(a => new AuditLogResponse(
            a.Id, a.TenantId, a.TenantName, a.EntityType, a.EntityId, a.Operation,
            a.PerformedByUserId, a.PerformedByEmail, a.IpAddress, a.CorrelationId, a.CreatedOn)).ToList();

        return new PaginatedList<AuditLogResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
