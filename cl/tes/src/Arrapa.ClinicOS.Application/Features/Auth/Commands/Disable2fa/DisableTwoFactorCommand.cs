using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.Disable2fa;

public sealed record DisableTwoFactorCommand(string VerificationCode) : IRequest<Result>;
