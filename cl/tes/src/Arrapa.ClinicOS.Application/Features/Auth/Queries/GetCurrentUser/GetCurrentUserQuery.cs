using Arrapa.ClinicOS.Contracts.Auth;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;

namespace Arrapa.ClinicOS.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<AuthUserResponse>>;
