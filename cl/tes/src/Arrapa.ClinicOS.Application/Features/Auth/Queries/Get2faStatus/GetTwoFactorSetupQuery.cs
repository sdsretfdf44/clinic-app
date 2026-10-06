using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Queries.Get2faStatus;

/// <summary>
/// Returns 2FA status for the current user. If not yet enabled, also returns a fresh
/// otpauth URI + shared key the user can enroll with by scanning a QR code.
/// </summary>
public sealed record GetTwoFactorSetupQuery : IRequest<Result<GetTwoFactorSetupResponse>>;

public sealed record GetTwoFactorSetupResponse(TwoFactorStatusResponse Status, TwoFactorSetupResponse? Setup);
