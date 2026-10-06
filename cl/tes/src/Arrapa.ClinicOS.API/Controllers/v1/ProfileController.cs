using Arrapa.ClinicOS.Application.Features.Auth.Commands.UpdateProfile;
using Arrapa.ClinicOS.Application.Features.Auth.Queries.GetCurrentUser;
using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Shared.Results;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1;

/// <summary>
/// Endpoints for the authenticated user's own profile.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profile")]
[Authorize]
public sealed class ProfileController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AuthUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCurrentUserQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<AuthUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand(request.FirstName, request.LastName, request.PhoneNumber, request.AvatarUrl);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Profile updated.");
    }
}
