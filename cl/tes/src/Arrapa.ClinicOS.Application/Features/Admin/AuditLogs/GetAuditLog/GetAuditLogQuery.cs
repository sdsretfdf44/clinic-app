using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.AuditLogs;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.AuditLogs.GetAuditLog;

public sealed record GetAuditLogQuery(Guid Id) : IRequest<Result<AuditLogDetailResponse>>;

public sealed class GetAuditLogQueryHandler : IRequestHandler<GetAuditLogQuery, Result<AuditLogDetailResponse>>
{
    private readonly IApplicationDbContext _db;

    public GetAuditLogQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<AuditLogDetailResponse>> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
    {
        var log = await _db.AuditLogs.AsNoTracking().IgnoreQueryFilters()
            .Where(a => a.Id == request.Id)
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
                a.UserAgent,
                a.CorrelationId,
                a.ChangesJson,
                a.CreatedOn
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (log is null)
        {
            return Shared.Results.Error.NotFound("admin.audit.not_found", "Audit log entry not found.");
        }

        return new AuditLogDetailResponse(
            log.Id, log.TenantId, log.TenantName, log.EntityType, log.EntityId, log.Operation,
            log.PerformedByUserId, log.PerformedByEmail, log.IpAddress, log.UserAgent, log.CorrelationId,
            log.ChangesJson, log.CreatedOn);
    }
}
