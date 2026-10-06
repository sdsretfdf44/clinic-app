using System.Security.Cryptography;
using Arrapa.ClinicOS.Application.Abstractions.Email;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Contracts.Admin.Tenants;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Domain.Tenants;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Application.Features.Admin.Tenants.CreateTenant;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantDetailResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        IApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IDateTimeProvider clock,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _db = db;
        _userManager = userManager;
        _emailService = emailService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<TenantDetailResponse>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var slugExists = await _db.Tenants.IgnoreQueryFilters()
            .AnyAsync(t => t.Slug == slug, cancellationToken).ConfigureAwait(false);
        if (slugExists)
        {
            return AdminErrors.Tenants.SlugAlreadyExists;
        }

        var normalizedAdminEmail = request.AdminEmail.Trim().ToUpperInvariant();
        var emailInUse = await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.NormalizedEmail == normalizedAdminEmail && !u.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (emailInUse)
        {
            return AdminErrors.Tenants.AdminEmailInUse;
        }

        Guid? planId = request.SubscriptionPlanId;
        if (planId is null)
        {
            planId = await _db.SubscriptionPlans.IgnoreQueryFilters()
                .Where(p => p.IsActive && p.IsDefault).Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var planExists = await _db.SubscriptionPlans.IgnoreQueryFilters()
                .AnyAsync(p => p.Id == planId && p.IsActive, cancellationToken).ConfigureAwait(false);
            if (!planExists)
            {
                return AdminErrors.Plans.NotFound;
            }
        }

        var trialDaysSetting = await _db.GlobalSettings.IgnoreQueryFilters()
            .Where(s => s.Key == "platform.trial_days").Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        var trialDays = int.TryParse(trialDaysSetting, out var parsedTrial) ? parsedTrial : 14;

        var now = _clock.UtcNow;
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            Status = TenantStatus.Trial,
            ContactEmail = request.ContactEmail?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            TimeZone = request.TimeZone.Trim(),
            Locale = request.Locale.Trim(),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            SubscriptionPlanId = planId,
            SubscriptionExpiresAt = now.AddDays(trialDays),
            CreatedOn = now
        };
        tenant.TenantId = tenant.Id;
        await _db.Tenants.AddAsync(tenant, cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var tempPassword = GenerateTemporaryPassword();
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.AdminEmail.Trim(),
            Email = request.AdminEmail.Trim(),
            EmailConfirmed = true,
            FirstName = request.AdminFirstName.Trim(),
            LastName = request.AdminLastName.Trim(),
            TenantId = tenant.Id,
            PrimaryRole = UserRole.ClinicAdmin,
            IsActive = true,
            CreatedOn = now
        };

        var createResult = await _userManager.CreateAsync(adminUser, tempPassword).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            var details = createResult.Errors
                .GroupBy(_ => "adminEmail", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray(), StringComparer.OrdinalIgnoreCase);
            return Error.Validation("admin.tenant.admin_create_failed", "Could not create the initial admin user.", details);
        }

        await _userManager.AddToRoleAsync(adminUser, AppConstants.Roles.ClinicAdmin).ConfigureAwait(false);

        await _emailService.SendAsync(new EmailMessage(
            To: adminUser.Email!,
            Subject: $"Welcome to {tenant.Name} on Arrapa ClinicOS",
            HtmlBody: $"""
                       <p>Hi {System.Net.WebUtility.HtmlEncode(adminUser.FirstName)},</p>
                       <p>Your Arrapa ClinicOS workspace <strong>{System.Net.WebUtility.HtmlEncode(tenant.Name)}</strong> is ready.</p>
                       <p>Sign in with:</p>
                       <ul>
                         <li><strong>Tenant:</strong> {tenant.Slug}</li>
                         <li><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(adminUser.Email!)}</li>
                         <li><strong>Temporary password:</strong> {tempPassword}</li>
                       </ul>
                       <p>Please change your password immediately after signing in.</p>
                       """,
            TextBody: $"Tenant: {tenant.Slug}. Email: {adminUser.Email}. Temp password: {tempPassword}",
            ToDisplayName: adminUser.FullName), cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Provisioned tenant {Slug} ({Id}) with admin {Email}.", tenant.Slug, tenant.Id, adminUser.Email);

        var planName = planId is null
            ? null
            : await _db.SubscriptionPlans.IgnoreQueryFilters()
                .Where(p => p.Id == planId).Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return new TenantDetailResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Status.ToString(),
            tenant.ContactEmail,
            tenant.ContactPhone,
            tenant.LogoUrl,
            tenant.TimeZone,
            tenant.Locale,
            tenant.Currency,
            tenant.SubscriptionPlanId,
            planName,
            tenant.SubscriptionExpiresAt,
            tenant.SuspendedAt,
            tenant.SuspensionReason,
            UserCount: 1,
            tenant.CreatedOn,
            tenant.UpdatedOn);
    }

    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*";

        Span<char> buffer = stackalloc char[14];
        buffer[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        buffer[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        buffer[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        buffer[3] = symbols[RandomNumberGenerator.GetInt32(symbols.Length)];

        const string all = upper + lower + digits + symbols;
        for (var i = 4; i < buffer.Length; i++)
        {
            buffer[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        // Fisher-Yates shuffle so mandatory chars are randomly positioned.
        for (var i = buffer.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
        }

        return new string(buffer);
    }
}
