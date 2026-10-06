using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email, string? TenantSlug, string ResetUrlTemplate) : IRequest<Result>;
