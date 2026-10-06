using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
