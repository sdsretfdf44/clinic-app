using Arrapa.ClinicOS.Application.Abstractions.Email;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Patients;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Application.Errors;
using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Shared.Constants;
using Arrapa.ClinicOS.Shared.Results;
using Arrapa.ClinicOS.Shared.Time;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.RegisterPatient;

/// <summary>
/// Creates a Patient user + Patient record in the target tenant and emails a verification link.
/// Password requirements are enforced by ASP.NET Identity; extra rules live in the validator.
/// </summary>
public sealed class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantLookupService _tenantLookup;
    private readonly IApplicationDbContext _db;
    private readonly IPatientMrnGenerator _mrnGenerator;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<RegisterPatientCommandHandler> _logger;

    public RegisterPatientCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITenantLookupService tenantLookup,
        IApplicationDbContext db,
        IPatientMrnGenerator mrnGenerator,
        IEmailService emailService,
        IDateTimeProvider clock,
        ILogger<RegisterPatientCommandHandler> logger)
    {
        _userManager = userManager;
        _tenantLookup = tenantLookup;
        _db = db;
        _mrnGenerator = mrnGenerator;
        _emailService = emailService;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantLookup.FindBySlugAsync(request.TenantSlug, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return AuthErrors.TenantNotFound;
        }

        var normalized = request.Email.Trim().ToUpperInvariant();
        var emailInUse = await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.TenantId == tenant.Id && u.NormalizedEmail == normalized && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (emailInUse)
        {
            return AuthErrors.EmailAlreadyRegistered;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            EmailConfirmed = false,
            PhoneNumber = request.Phone.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            TenantId = tenant.Id,
            PrimaryRole = UserRole.Patient,
            IsActive = true,
            CreatedOn = _clock.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            var details = createResult.Errors
                .GroupBy(_ => "password", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray(), StringComparer.OrdinalIgnoreCase);
            _logger.LogInformation("Patient registration failed for {Email}: {Errors}", request.Email, string.Join(';', createResult.Errors.Select(e => e.Description)));
            return Error.Validation(AuthErrors.RegistrationFailed.Code, AuthErrors.RegistrationFailed.Message, details);
        }

        await _userManager.AddToRoleAsync(user, AppConstants.Roles.Patient).ConfigureAwait(false);

        var mrn = await _mrnGenerator.GenerateAsync(tenant.Id, cancellationToken).ConfigureAwait(false);
        var patient = new Patient
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            MRN = mrn,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            DateOfBirth = request.DateOfBirth ?? default,
            CreatedOn = _clock.UtcNow,
        };
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
        var url = request.VerificationUrlTemplate
            .Replace("{token}", Uri.EscapeDataString(token), StringComparison.OrdinalIgnoreCase)
            .Replace("{email}", Uri.EscapeDataString(user.Email!), StringComparison.OrdinalIgnoreCase)
            .Replace("{tenant}", tenant.Slug, StringComparison.OrdinalIgnoreCase);

        await _emailService.SendAsync(new EmailMessage(
            To: user.Email!,
            Subject: $"Confirm your {tenant.Name} account",
            HtmlBody: $"""
                       <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
                       <p>Welcome to <strong>{System.Net.WebUtility.HtmlEncode(tenant.Name)}</strong>. Please confirm your email to activate your account.</p>
                       <p><a href="{url}">Click here to verify your email</a>. This link expires in 24 hours.</p>
                       <p>If you didn't create this account, you can safely ignore this message.</p>
                       """,
            TextBody: $"Confirm your email: {url}",
            ToDisplayName: user.FullName), cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Patient {UserId} registered on tenant {Tenant} (email pending verification).", user.Id, tenant.Slug);
        return Result.Success();
    }
}
