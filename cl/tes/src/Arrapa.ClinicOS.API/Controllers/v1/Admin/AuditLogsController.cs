using Arrapa.ClinicOS.Application.Features.Admin.AuditLogs.GetAuditLog;
using Arrapa.ClinicOS.Application.Features.Admin.AuditLogs.ListAuditLogs;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Admin;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/audit-logs")]
public sealed class AuditLogsController : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? operation = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListAuditLogsQuery(pageNumber, pageSize, tenantId, entityType, operation, from, to, search);
        var result = await Sender.Send(query, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAuditLogQuery(id), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }
}
