using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string TenantSlug, string Email, string Token) : IRequest<Result>;
