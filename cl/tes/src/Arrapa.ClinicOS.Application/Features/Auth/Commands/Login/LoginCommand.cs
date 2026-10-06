using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.Login;

/// <summary>
/// Authenticates a user against the specified tenant and returns an access + refresh token pair.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? TenantSlug,
    string? IpAddress,
    string? UserAgent,
    string? TwoFactorCode = null) : IRequest<Result<AuthTokensResponse>>;
