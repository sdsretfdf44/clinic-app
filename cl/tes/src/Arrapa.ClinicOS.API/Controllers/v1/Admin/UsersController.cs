using Arrapa.ClinicOS.Application.Features.Admin.Users.ListUsers;
using Arrapa.ClinicOS.Application.Features.Admin.Users.SetUserActive;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Admin;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/users")]
public sealed class UsersController : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListUsersQuery(pageNumber, pageSize, search, tenantId, role, isActive), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetUserActiveCommand(id, IsActive: true), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "User activated.");
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetUserActiveCommand(id, IsActive: false), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "User deactivated.");
    }
}
