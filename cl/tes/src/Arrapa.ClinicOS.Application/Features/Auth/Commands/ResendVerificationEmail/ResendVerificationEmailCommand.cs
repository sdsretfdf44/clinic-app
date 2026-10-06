using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ResendVerificationEmail;

public sealed record ResendVerificationEmailCommand(
    string TenantSlug,
    string Email,
    string VerificationUrlTemplate) : IRequest<Result>;
