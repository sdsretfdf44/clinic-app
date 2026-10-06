using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Clinic.Common;
using Arrapa.ClinicOS.Contracts.Clinic.Users;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Users;

// -------------------- List --------------------
public sealed record ListClinicUsersQuery(
    int PageNumber, int PageSize, string? Search, string? Role, bool? IsActive)
    : IRequest<Result<PaginatedList<ClinicUserResponse>>>;

public sealed class ListClinicUsersQueryHandler : IRequestHandler<ListClinicUsersQuery, Result<PaginatedList<ClinicUserResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenantContext;

    public ListClinicUsersQueryHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ITenantContext tenantContext)
    {
        _db = db;
        _userManager = userManager;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PaginatedList<ClinicUserResponse>>> Handle(ListClinicUsersQuery request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<PaginatedList<ClinicUserResponse>>.Failure(tenantIdResult.Error);
        }

        var tenantId = tenantIdResult.Value;
        var query = _db.Users.IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId && !u.IsDeleted);

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.NormalizedEmail!.Contains(term.ToUpper()) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var pageUsers = await query
            .OrderByDescending(u => u.CreatedOn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var items = new List<ClinicUserResponse>(pageUsers.Count);
        foreach (var user in pageUsers)
        {
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(request.Role) && !roles.Contains(request.Role))
            {
                continue;
            }
            var lockedOut = await _userManager.IsLockedOutAsync(user).ConfigureAwait(false);
            items.Add(new ClinicUserResponse(
                user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, user.FullName,
                user.PhoneNumber, user.PrimaryRole.ToString(), roles.ToArray(),
                user.IsActive, lockedOut, user.LastLoginAt, user.CreatedOn));
        }

        return new PaginatedList<ClinicUserResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }
}

// -------------------- Invite user --------------------
public sealed record InviteClinicUserCommand(string FirstName, string LastName, string Email, string PrimaryRole)
    : IRequest<Result<ClinicUserResponse>>;

public sealed class InviteClinicUserCommandValidator : AbstractValidator<InviteClinicUserCommand>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        AppConstants.Roles.ClinicAdmin, AppConstants.Roles.Doctor,
        AppConstants.Roles.Receptionist, AppConstants.Roles.Patient
    };

    public InviteClinicUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PrimaryRole).Must(r => AllowedRoles.Contains(r))
            .WithMessage("Role must be one of: ClinicAdmin, Doctor, Receptionist, Patient.");
    }
}

public sealed class InviteClinicUserCommandHandler : IRequestHandler<InviteClinicUserCommand, Result<ClinicUserResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ClinicUserProvisioning _provisioning;

    public InviteClinicUserCommandHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ITenantContext tenantContext,
        ClinicUserProvisioning provisioning)
    {
        _db = db;
        _userManager = userManager;
        _tenantContext = tenantContext;
        _provisioning = provisioning;
    }

    public async Task<Result<ClinicUserResponse>> Handle(InviteClinicUserCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<ClinicUserResponse>.Failure(tenantIdResult.Error);
        }

        var role = Enum.Parse<UserRole>(request.PrimaryRole, ignoreCase: true);

        var userResult = await _provisioning.ProvisionAsync(
            tenantIdResult.Value, request.FirstName, request.LastName, request.Email, phoneNumber: null,
            role, request.PrimaryRole, ClinicErrors.Users.EmailInUse, cancellationToken).ConfigureAwait(false);

        if (userResult.IsFailure)
        {
            return Result<ClinicUserResponse>.Failure(userResult.Error);
        }

        var roles = await _userManager.GetRolesAsync(userResult.Value).ConfigureAwait(false);
        return new ClinicUserResponse(
            userResult.Value.Id, userResult.Value.Email ?? string.Empty,
            userResult.Value.FirstName, userResult.Value.LastName, userResult.Value.FullName,
            userResult.Value.PhoneNumber, userResult.Value.PrimaryRole.ToString(), roles.ToArray(),
            userResult.Value.IsActive, IsLockedOut: false, userResult.Value.LastLoginAt, userResult.Value.CreatedOn);
    }
}

// -------------------- Change role --------------------
public sealed record ChangeClinicUserRoleCommand(Guid UserId, string PrimaryRole) : IRequest<Result>;

public sealed class ChangeClinicUserRoleCommandValidator : AbstractValidator<ChangeClinicUserRoleCommand>
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        AppConstants.Roles.ClinicAdmin, AppConstants.Roles.Doctor,
        AppConstants.Roles.Receptionist, AppConstants.Roles.Patient
    };

    public ChangeClinicUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PrimaryRole).Must(r => AllowedRoles.Contains(r))
            .WithMessage("Role must be one of: ClinicAdmin, Doctor, Receptionist, Patient.");
    }
}

public sealed class ChangeClinicUserRoleCommandHandler : IRequestHandler<ChangeClinicUserRoleCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;

    public ChangeClinicUserRoleCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITenantContext tenantContext,
        ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangeClinicUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == request.UserId)
        {
            return ClinicErrors.Users.CannotChangeOwnRole;
        }

        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return tenantIdResult.Error;
        }

        var user = await _userManager.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == tenantIdResult.Value && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return ClinicErrors.Users.NotFound;
        }

        var newRole = Enum.Parse<UserRole>(request.PrimaryRole, ignoreCase: true);

        var currentRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles).ConfigureAwait(false);
        }
        await _userManager.AddToRoleAsync(user, request.PrimaryRole).ConfigureAwait(false);

        user.PrimaryRole = newRole;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);
        return Result.Success();
    }
}

// -------------------- Toggle active --------------------
public sealed record SetClinicUserActiveCommand(Guid UserId, bool IsActive) : IRequest<Result>;

public sealed class SetClinicUserActiveCommandHandler : IRequestHandler<SetClinicUserActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public SetClinicUserActiveCommandHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ITenantContext tenantContext,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _userManager = userManager;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(SetClinicUserActiveCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == request.UserId && !request.IsActive)
        {
            return ClinicErrors.Users.CannotDeactivateSelf;
        }

        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return tenantIdResult.Error;
        }

        var user = await _userManager.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == tenantIdResult.Value && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return ClinicErrors.Users.NotFound;
        }

        user.IsActive = request.IsActive;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);

        if (!request.IsActive)
        {
            var tokens = await _db.RefreshTokens.IgnoreQueryFilters()
                .Where(t => t.UserId == user.Id && t.RevokedAt == null)
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
