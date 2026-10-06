using System.Security.Cryptography;
using Arrapa.ClinicOS.Application.Abstractions.Email;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Arrapa.ClinicOS.Application.Features.Clinic.Common;

/// <summary>
/// Provisions a new <see cref="ApplicationUser"/> inside a tenant and assigns the requested role. Used by the Doctor
/// / Receptionist / Invite-user flows so they share the same email-uniqueness check, temporary-password logic,
/// and welcome email.
/// </summary>
public sealed class ClinicUserProvisioning
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _clock;

    public ClinicUserProvisioning(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext db,
        IEmailService emailService,
        IDateTimeProvider clock)
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
        _clock = clock;
    }

    public async Task<Result<ApplicationUser>> ProvisionAsync(
        Guid tenantId,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber,
        UserRole primaryRole,
        string roleName,
        Error emailInUseError,
        CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToUpperInvariant();

        var emailInUse = await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.TenantId == tenantId && u.NormalizedEmail == normalized && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (emailInUse)
        {
            return Result<ApplicationUser>.Failure(emailInUseError);
        }

        var tempPassword = GenerateTemporaryPassword();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            TenantId = tenantId,
            PrimaryRole = primaryRole,
            IsActive = true,
            CreatedOn = _clock.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, tempPassword).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            var details = createResult.Errors
                .GroupBy(_ => "email", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray(), StringComparer.OrdinalIgnoreCase);
            return Error.Validation("clinic.user.create_failed", "Could not create user.", details);
        }

        await _userManager.AddToRoleAsync(user, roleName).ConfigureAwait(false);

        var tenantName = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantId).Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "Arrapa ClinicOS";
        var tenantSlug = await _db.Tenants.AsNoTracking().IgnoreQueryFilters()
            .Where(t => t.Id == tenantId).Select(t => t.Slug)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? "arrapa";

        await _emailService.SendAsync(new EmailMessage(
            To: user.Email!,
            Subject: $"You have been invited to {tenantName} on Arrapa ClinicOS",
            HtmlBody: $"""
                       <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
                       <p>You have been added to <strong>{System.Net.WebUtility.HtmlEncode(tenantName)}</strong> as {roleName}.</p>
                       <p>Sign in with:</p>
                       <ul>
                         <li><strong>Tenant:</strong> {tenantSlug}</li>
                         <li><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(user.Email!)}</li>
                         <li><strong>Temporary password:</strong> {tempPassword}</li>
                       </ul>
                       <p>Please change your password on first sign-in.</p>
                       """,
            TextBody: $"Tenant: {tenantSlug}. Email: {user.Email}. Temp password: {tempPassword}",
            ToDisplayName: user.FullName), cancellationToken).ConfigureAwait(false);

        return user;
    }

    public static string GenerateTemporaryPassword()
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

        for (var i = buffer.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
        }

        return new string(buffer);
    }
}
