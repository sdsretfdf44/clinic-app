using Arrapa.ClinicOS.Application.Features.Clinic.Profile.GetClinicProfile;
using Arrapa.ClinicOS.Application.Features.Clinic.Profile.UpdateClinicProfile;
using Arrapa.ClinicOS.Contracts.Clinic.Profile;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Arrapa.ClinicOS.API.Controllers.v1.Clinic;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clinic/profile")]
public sealed class ProfileController : ClinicControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetClinicProfileQuery(), cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateClinicProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateClinicProfileCommand(
            request.Name, request.ContactEmail, request.ContactPhone, request.LogoUrl,
            request.TimeZone, request.Locale, request.Currency, request.DefaultConsultationFee);
        var result = await Sender.Send(command, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, "Clinic profile updated.");
    }
}
