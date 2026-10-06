using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(string FirstName, string LastName, string? PhoneNumber, string? AvatarUrl)
    : IRequest<Result<AuthUserResponse>>;
