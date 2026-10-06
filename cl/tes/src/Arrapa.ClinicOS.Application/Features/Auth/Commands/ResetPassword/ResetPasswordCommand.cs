using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword, string? TenantSlug) : IRequest<Result>;
