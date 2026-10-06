using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Users.SetUserActive;

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest<Result>;

public sealed class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public SetUserActiveCommandHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _userManager = userManager;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == request.UserId && !request.IsActive)
        {
            return AdminErrors.Users.CannotDeactivateSelf;
        }

        var user = await _userManager.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return AdminErrors.Users.NotFound;
        }

        user.IsActive = request.IsActive;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);

        if (!request.IsActive)
        {
            var tokens = await _db.RefreshTokens.IgnoreQueryFilters()
                .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var now = _clock.UtcNow;
            foreach (var t in tokens)
            {
                t.RevokedAt = now;
            }
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}
