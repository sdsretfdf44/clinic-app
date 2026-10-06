using Arrapa.ClinicOS.Application.Features.Clinic.Users;
using Arrapa.ClinicOS.Contracts.Clinic.Users;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clinic/users")]
public sealed class UsersController : ClinicControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new ListClinicUsersQuery(pageNumber, pageSize, search, role, isActive), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Invite([FromBody] InviteUserRequest request, CancellationToken cancellationToken)
    {
        var command = new InviteClinicUserCommand(request.FirstName, request.LastName, request.Email, request.PrimaryRole);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "User invited. A temporary password has been sent to their email.");
    }

    [HttpPost("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ChangeClinicUserRoleCommand(id, request.PrimaryRole), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Role changed.");
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetClinicUserActiveCommand(id, IsActive: true), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "User activated.");
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new SetClinicUserActiveCommand(id, IsActive: false), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "User deactivated.");
    }
}
