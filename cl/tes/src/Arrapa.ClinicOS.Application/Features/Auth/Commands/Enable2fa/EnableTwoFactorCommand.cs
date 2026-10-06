using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.Enable2fa;

public sealed record EnableTwoFactorCommand(string VerificationCode) : IRequest<Result>;
