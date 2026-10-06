using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.RegisterPatient;

/// <summary>
/// Public self-service patient signup. Creates the ApplicationUser (Patient role) and the backing
/// Patient record inside the given tenant, then dispatches a verification email.
/// </summary>
public sealed record RegisterPatientCommand(
    string TenantSlug,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password,
    DateOnly? DateOfBirth,
    string VerificationUrlTemplate) : IRequest<Result>;
