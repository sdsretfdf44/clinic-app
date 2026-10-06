using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Rotates a refresh token: revokes the presented one and mints a new pair.
/// </summary>
public sealed record RefreshTokenCommand(string Token, string? IpAddress, string? UserAgent) : IRequest<Result<AuthTokensResponse>>;
