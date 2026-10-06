using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Contracts.Admin.Users;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Pagination;
using Arrapa.ClinicOS.Shared.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Admin.Users.ListUsers;

public sealed record ListUsersQuery(
    int PageNumber,
    int PageSize,
    string? Search,
    Guid? TenantId,
    string? Role,
    bool? IsActive) : IRequest<Result<PaginatedList<AdminUserResponse>>>;

public sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, Result<PaginatedList<AdminUserResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ListUsersQueryHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<Result<PaginatedList<AdminUserResponse>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Users.AsNoTracking().IgnoreQueryFilters().Where(u => !u.IsDeleted);

        if (request.TenantId.HasValue && request.TenantId != Guid.Empty)
        {
            query = query.Where(u => u.TenantId == request.TenantId);
        }

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

        var tenantIds = pageUsers.Select(u => u.TenantId).Distinct().ToList();
        var tenants = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => tenantIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Name, t.Slug })
            .ToDictionaryAsync(x => x.Id, cancellationToken).ConfigureAwait(false);

        var items = new List<AdminUserResponse>(pageUsers.Count);
        foreach (var user in pageUsers)
        {
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(request.Role) && !roles.Contains(request.Role))
            {
                continue;
            }
            var lockedOut = await _userManager.IsLockedOutAsync(user).ConfigureAwait(false);
            var tenantInfo = tenants.GetValueOrDefault(user.TenantId);
            items.Add(new AdminUserResponse(
                user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, user.FullName,
                user.PhoneNumber, user.TenantId,
                tenantInfo?.Name ?? "(deleted tenant)", tenantInfo?.Slug ?? "-",
                user.PrimaryRole.ToString(), roles.ToArray(),
                user.IsActive, lockedOut, user.LastLoginAt, user.CreatedOn));
        }

        return new PaginatedList<AdminUserResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
