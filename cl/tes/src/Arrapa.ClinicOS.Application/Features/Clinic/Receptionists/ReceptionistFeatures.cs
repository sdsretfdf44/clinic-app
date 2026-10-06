using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Application.Features.Clinic.Common;
using Arrapa.ClinicOS.Contracts.Clinic.Receptionists;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Receptionists;

// -------------------- List --------------------
public sealed record ListReceptionistsQuery(bool IncludeInactive) : IRequest<Result<IReadOnlyList<ReceptionistResponse>>>;

public sealed class ListReceptionistsQueryHandler : IRequestHandler<ListReceptionistsQuery, Result<IReadOnlyList<ReceptionistResponse>>>
{
    private readonly IApplicationDbContext _db;

    public ListReceptionistsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<ReceptionistResponse>>> Handle(ListReceptionistsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Receptionists.AsNoTracking().Include(r => r.User).AsQueryable();
        if (!request.IncludeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        var rows = await query
            .OrderBy(r => r.SortOrder).ThenBy(r => r.User!.LastName)
            .Select(r => new
            {
                r.Id,
                r.UserId,
                r.User!.FirstName,
                r.User!.LastName,
                Email = r.User!.Email ?? string.Empty,
                r.User!.PhoneNumber,
                r.User!.AvatarUrl,
                r.Title,
                r.IsActive,
                r.SortOrder,
                r.CreatedOn
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var items = rows.Select(r => new ReceptionistResponse(
            r.Id, r.UserId, r.FirstName, r.LastName,
            $"{r.FirstName} {r.LastName}".Trim(),
            r.Email, r.PhoneNumber, r.AvatarUrl,
            r.Title, r.IsActive, r.SortOrder, r.CreatedOn)).ToList();

        return items;
    }
}

// -------------------- Create --------------------
public sealed record CreateReceptionistCommand(string FirstName, string LastName, string Email, string? PhoneNumber, string? Title) : IRequest<Result<ReceptionistResponse>>;

public sealed class CreateReceptionistCommandValidator : AbstractValidator<CreateReceptionistCommand>
{
    public CreateReceptionistCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.Title).MaximumLength(200);
    }
}

public sealed class CreateReceptionistCommandHandler : IRequestHandler<CreateReceptionistCommand, Result<ReceptionistResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ClinicUserProvisioning _provisioning;

    public CreateReceptionistCommandHandler(
        IApplicationDbContext db,
        ITenantContext tenantContext,
        ClinicUserProvisioning provisioning)
    {
        _db = db;
        _tenantContext = tenantContext;
        _provisioning = provisioning;
    }

    public async Task<Result<ReceptionistResponse>> Handle(CreateReceptionistCommand request, CancellationToken cancellationToken)
    {
        var tenantIdResult = _tenantContext.RequireTenantId();
        if (tenantIdResult.IsFailure)
        {
            return Result<ReceptionistResponse>.Failure(tenantIdResult.Error);
        }

        var userResult = await _provisioning.ProvisionAsync(
            tenantIdResult.Value, request.FirstName, request.LastName, request.Email, request.PhoneNumber,
            UserRole.Receptionist, AppConstants.Roles.Receptionist, ClinicErrors.Receptionists.EmailInUse, cancellationToken)
            .ConfigureAwait(false);
        if (userResult.IsFailure)
        {
            return Result<ReceptionistResponse>.Failure(userResult.Error);
        }

        var receptionist = new Receptionist
        {
            Id = Guid.NewGuid(),
            TenantId = tenantIdResult.Value,
            UserId = userResult.Value.Id,
            Title = request.Title?.Trim(),
            IsActive = true
        };
        await _db.Receptionists.AddAsync(receptionist, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ReceptionistResponse(
            receptionist.Id, receptionist.UserId,
            userResult.Value.FirstName, userResult.Value.LastName, userResult.Value.FullName,
            userResult.Value.Email ?? string.Empty, userResult.Value.PhoneNumber, userResult.Value.AvatarUrl,
            receptionist.Title, receptionist.IsActive, receptionist.SortOrder, receptionist.CreatedOn);
    }
}

// -------------------- Update --------------------
public sealed record UpdateReceptionistCommand(
    Guid Id, string FirstName, string LastName, string? PhoneNumber, string? AvatarUrl,
    string? Title, int SortOrder) : IRequest<Result<ReceptionistResponse>>;

public sealed class UpdateReceptionistCommandValidator : AbstractValidator<UpdateReceptionistCommand>
{
    public UpdateReceptionistCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.AvatarUrl).MaximumLength(500);
        RuleFor(x => x.Title).MaximumLength(200);
    }
}

public sealed class UpdateReceptionistCommandHandler : IRequestHandler<UpdateReceptionistCommand, Result<ReceptionistResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDateTimeProvider _clock;

    public UpdateReceptionistCommandHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IDateTimeProvider clock)
    {
        _db = db;
        _userManager = userManager;
        _clock = clock;
    }

    public async Task<Result<ReceptionistResponse>> Handle(UpdateReceptionistCommand request, CancellationToken cancellationToken)
    {
        var receptionist = await _db.Receptionists.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (receptionist is null || receptionist.User is null)
        {
            return ClinicErrors.Receptionists.NotFound;
        }

        receptionist.User.FirstName = request.FirstName.Trim();
        receptionist.User.LastName = request.LastName.Trim();
        receptionist.User.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        receptionist.User.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        await _userManager.UpdateAsync(receptionist.User).ConfigureAwait(false);

        receptionist.Title = request.Title?.Trim();
        receptionist.SortOrder = request.SortOrder;
        receptionist.UpdatedOn = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ReceptionistResponse(
            receptionist.Id, receptionist.UserId,
            receptionist.User.FirstName, receptionist.User.LastName, receptionist.User.FullName,
            receptionist.User.Email ?? string.Empty, receptionist.User.PhoneNumber, receptionist.User.AvatarUrl,
            receptionist.Title, receptionist.IsActive, receptionist.SortOrder, receptionist.CreatedOn);
    }
}

// -------------------- Toggle Active --------------------
public sealed record SetReceptionistActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;

public sealed class SetReceptionistActiveCommandHandler : IRequestHandler<SetReceptionistActiveCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDateTimeProvider _clock;

    public SetReceptionistActiveCommandHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IDateTimeProvider clock)
    {
        _db = db;
        _userManager = userManager;
        _clock = clock;
    }

    public async Task<Result> Handle(SetReceptionistActiveCommand request, CancellationToken cancellationToken)
    {
        var receptionist = await _db.Receptionists.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (receptionist is null || receptionist.User is null)
        {
            return ClinicErrors.Receptionists.NotFound;
        }

        receptionist.IsActive = request.IsActive;
        receptionist.User.IsActive = request.IsActive;
        await _userManager.UpdateAsync(receptionist.User).ConfigureAwait(false);

        if (!request.IsActive)
        {
            var tokens = await _db.RefreshTokens.IgnoreQueryFilters()
                .Where(t => t.UserId == receptionist.UserId && t.RevokedAt == null)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var now = _clock.UtcNow;
            foreach (var t in tokens)
            {
                t.RevokedAt = now;
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

// -------------------- Delete --------------------
public sealed record DeleteReceptionistCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteReceptionistCommandHandler : IRequestHandler<DeleteReceptionistCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteReceptionistCommandHandler(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<Result> Handle(DeleteReceptionistCommand request, CancellationToken cancellationToken)
    {
        var receptionist = await _db.Receptionists.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken).ConfigureAwait(false);
        if (receptionist is null)
        {
            return ClinicErrors.Receptionists.NotFound;
        }

        _db.Receptionists.Remove(receptionist);
        if (receptionist.User is not null)
        {
            receptionist.User.IsActive = false;
            await _userManager.UpdateAsync(receptionist.User).ConfigureAwait(false);
        }
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
