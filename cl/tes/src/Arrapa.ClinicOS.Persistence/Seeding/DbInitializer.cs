using Arrapa.ClinicOS.Domain.Clinical;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Domain.FeatureFlags;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Domain.Settings;
using Arrapa.ClinicOS.Domain.Subscriptions;
using Arrapa.ClinicOS.Domain.Tenants;
using Arrapa.ClinicOS.Persistence.Context;
using Arrapa.ClinicOS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arrapa.ClinicOS.Persistence.Seeding;

/// <summary>
/// Applies pending EF Core migrations and seeds the platform tenant, roles, and the initial super admin account.
/// Idempotent: safe to run on every application start.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<ApplicationRole>>();

        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Seeding subscription plans...");
        await SeedSubscriptionPlansAsync(context, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Seeding platform tenant...");
        await SeedPlatformTenantAsync(context, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Seeding roles...");
        await SeedRolesAsync(roleManager).ConfigureAwait(false);

        logger.LogInformation("Seeding super admin account...");
        await SeedSuperAdminAsync(userManager, logger).ConfigureAwait(false);

        logger.LogInformation("Seeding feature flags...");
        await SeedFeatureFlagsAsync(context, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Seeding global settings...");
        await SeedGlobalSettingsAsync(context, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Seeding demo tenant + users...");
        await SeedDemoTenantAsync(context, userManager, logger, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Database initialization complete.");
    }

    private static async Task SeedPlatformTenantAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var platformId = AppConstants.SystemTenants.PlatformTenantId;
        var existing = await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == platformId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return;
        }

        var tenant = new Tenant
        {
            Id = platformId,
            TenantId = platformId,
            Name = "Arrapa Software",
            Slug = AppConstants.SystemTenants.PlatformTenantSlug,
            Status = TenantStatus.Active,
            ContactEmail = "platform@arrapa.software",
            TimeZone = "UTC",
            Locale = "en-US",
            Currency = "USD",
            CreatedOn = DateTimeOffset.UtcNow
        };
        await context.Tenants.AddAsync(tenant, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var roles = new[]
        {
            AppConstants.Roles.SuperAdmin,
            AppConstants.Roles.ClinicAdmin,
            AppConstants.Roles.Doctor,
            AppConstants.Roles.Receptionist,
            AppConstants.Roles.Patient
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
            {
                var role = new ApplicationRole(roleName)
                {
                    IsSystemRole = true,
                    Description = $"System role: {roleName}"
                };
                await roleManager.CreateAsync(role).ConfigureAwait(false);
            }
        }
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string email = "superadmin@arrapa.software";
        const string defaultPassword = "ChangeMe!2026";

        var existing = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (existing is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Platform",
            LastName = "Administrator",
            TenantId = AppConstants.SystemTenants.PlatformTenantId,
            PrimaryRole = UserRole.SuperAdmin,
            IsActive = true,
            CreatedOn = DateTimeOffset.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, defaultPassword).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            logger.LogError("Failed to seed super admin: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(user, AppConstants.Roles.SuperAdmin).ConfigureAwait(false);
        logger.LogWarning("Seeded default super admin. Email: {Email}. Password: {Password}. Change on first login.", email, defaultPassword);
    }

    private static async Task SeedSubscriptionPlansAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.SubscriptionPlans.IgnoreQueryFilters().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var platformId = AppConstants.SystemTenants.PlatformTenantId;
        var now = DateTimeOffset.UtcNow;

        var plans = new[]
        {
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                TenantId = platformId,
                Code = "starter",
                Name = "Starter",
                Description = "For small clinics getting started with digital operations.",
                Interval = SubscriptionInterval.Monthly,
                Price = 49m,
                Currency = "USD",
                MaxUsers = 5,
                MaxPatients = 500,
                MaxAppointmentsPerMonth = 1000,
                FeaturesJson = "[\"appointments\",\"patients\",\"prescriptions\"]",
                IsActive = true,
                IsDefault = true,
                SortOrder = 10,
                CreatedOn = now
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                TenantId = platformId,
                Code = "growth",
                Name = "Growth",
                Description = "For growing clinics adding staff and specialists.",
                Interval = SubscriptionInterval.Monthly,
                Price = 149m,
                Currency = "USD",
                MaxUsers = 20,
                MaxPatients = 5000,
                MaxAppointmentsPerMonth = 10_000,
                FeaturesJson = "[\"appointments\",\"patients\",\"prescriptions\",\"billing\",\"reports\"]",
                IsActive = true,
                IsDefault = false,
                SortOrder = 20,
                CreatedOn = now
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                TenantId = platformId,
                Code = "enterprise",
                Name = "Enterprise",
                Description = "Custom limits, SLA, and integrations for hospital networks.",
                Interval = SubscriptionInterval.Monthly,
                Price = 499m,
                Currency = "USD",
                MaxUsers = null,
                MaxPatients = null,
                MaxAppointmentsPerMonth = null,
                FeaturesJson = "[\"appointments\",\"patients\",\"prescriptions\",\"billing\",\"reports\",\"lis\",\"pharmacy\",\"telemedicine\",\"ai\"]",
                IsActive = true,
                IsDefault = false,
                SortOrder = 30,
                CreatedOn = now
            }
        };

        await context.SubscriptionPlans.AddRangeAsync(plans, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedFeatureFlagsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.FeatureFlags.IgnoreQueryFilters().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var platformId = AppConstants.SystemTenants.PlatformTenantId;
        var now = DateTimeOffset.UtcNow;

        var flags = new[]
        {
            new FeatureFlag { Id = Guid.NewGuid(), TenantId = platformId, Key = "telemedicine", Name = "Telemedicine", Description = "Enables video consultation flows.", DefaultEnabled = false, IsGlobal = false, CreatedOn = now },
            new FeatureFlag { Id = Guid.NewGuid(), TenantId = platformId, Key = "ai_assistant", Name = "AI Assistant", Description = "In-app AI clinical assistant.", DefaultEnabled = false, IsGlobal = false, CreatedOn = now },
            new FeatureFlag { Id = Guid.NewGuid(), TenantId = platformId, Key = "patient_portal", Name = "Patient Portal", Description = "Grants patients self-service access.", DefaultEnabled = true, IsGlobal = false, CreatedOn = now },
            new FeatureFlag { Id = Guid.NewGuid(), TenantId = platformId, Key = "email_notifications", Name = "Email Notifications", Description = "Enables outbound transactional emails.", DefaultEnabled = true, IsGlobal = true, CreatedOn = now }
        };

        await context.FeatureFlags.AddRangeAsync(flags, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task SeedGlobalSettingsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.GlobalSettings.IgnoreQueryFilters().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var platformId = AppConstants.SystemTenants.PlatformTenantId;
        var now = DateTimeOffset.UtcNow;

        var settings = new[]
        {
            new GlobalSetting { Id = Guid.NewGuid(), TenantId = platformId, Key = "platform.support_email", Value = "support@arrapa.software", Category = "General", DataType = SettingDataType.String, Description = "Displayed on error pages and outbound emails.", CreatedOn = now },
            new GlobalSetting { Id = Guid.NewGuid(), TenantId = platformId, Key = "platform.trial_days", Value = "14", Category = "Billing", DataType = SettingDataType.Integer, Description = "Trial period granted to new tenants.", CreatedOn = now },
            new GlobalSetting { Id = Guid.NewGuid(), TenantId = platformId, Key = "platform.maintenance_mode", Value = "false", Category = "General", DataType = SettingDataType.Boolean, Description = "When true, non-super-admin users see a maintenance page.", CreatedOn = now }
        };

        await context.GlobalSettings.AddRangeAsync(settings, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Seeds a demo tenant with a full clinical team so a fresh install can be poked immediately from the sign-in
    /// page. Idempotent: presence of the demo tenant slug is the trip-wire.
    /// </summary>
    private static async Task SeedDemoTenantAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        const string demoSlug = "demo";
        var existing = await context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == demoSlug, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return;
        }

        var defaultPlan = await context.SubscriptionPlans.IgnoreQueryFilters()
            .Where(p => p.IsDefault && p.IsActive).Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        var tenantId = Guid.NewGuid();

        var tenant = new Tenant
        {
            Id = tenantId,
            TenantId = tenantId,
            Name = "Demo Family Clinic",
            Slug = demoSlug,
            Status = TenantStatus.Active,
            ContactEmail = "hello@demo.clinic",
            ContactPhone = "+1-555-0100",
            TimeZone = "UTC",
            Locale = "en-US",
            Currency = "USD",
            SubscriptionPlanId = defaultPlan == Guid.Empty ? null : defaultPlan,
            SubscriptionExpiresAt = now.AddYears(1),
            DefaultConsultationFee = 50m,
            CreatedOn = now
        };
        await context.Tenants.AddAsync(tenant, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Seed a General Medicine department
        var department = new Department
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "General Medicine",
            Description = "Primary care and general consultations.",
            DefaultConsultationFee = 50m,
            IsActive = true,
            SortOrder = 10,
            CreatedOn = now
        };
        await context.Departments.AddAsync(department, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Seed users
        var adminUser = await CreateUserAsync(userManager, logger,
            email: "admin@demo.clinic", password: "ClinicAdmin!2026",
            firstName: "Alex", lastName: "Reyes",
            tenantId: tenantId, primaryRole: UserRole.ClinicAdmin,
            roleName: AppConstants.Roles.ClinicAdmin).ConfigureAwait(false);

        var doctorUser = await CreateUserAsync(userManager, logger,
            email: "doctor@demo.clinic", password: "Doctor!2026",
            firstName: "Priya", lastName: "Iyer",
            tenantId: tenantId, primaryRole: UserRole.Doctor,
            roleName: AppConstants.Roles.Doctor).ConfigureAwait(false);

        var receptionistUser = await CreateUserAsync(userManager, logger,
            email: "reception@demo.clinic", password: "Reception!2026",
            firstName: "Sam", lastName: "Brooks",
            tenantId: tenantId, primaryRole: UserRole.Receptionist,
            roleName: AppConstants.Roles.Receptionist).ConfigureAwait(false);

        if (doctorUser is not null)
        {
            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = doctorUser.Id,
                DepartmentId = department.Id,
                Specialty = "General Practitioner",
                LicenseNumber = "DEMO-LIC-001",
                ConsultationFee = 50m,
                Bio = "Board-certified general practitioner with 8 years of primary care experience.",
                IsAcceptingNewPatients = true,
                IsActive = true,
                SortOrder = 10,
                CreatedOn = now
            };
            await context.Doctors.AddAsync(doctor, cancellationToken).ConfigureAwait(false);
        }

        if (receptionistUser is not null)
        {
            var receptionist = new Receptionist
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = receptionistUser.Id,
                Title = "Front Desk Lead",
                IsActive = true,
                SortOrder = 10,
                CreatedOn = now
            };
            await context.Receptionists.AddAsync(receptionist, cancellationToken).ConfigureAwait(false);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogWarning(
            "Seeded demo tenant '{Slug}'. ClinicAdmin: admin@demo.clinic / ClinicAdmin!2026. " +
            "Doctor: doctor@demo.clinic / Doctor!2026. Receptionist: reception@demo.clinic / Reception!2026.",
            demoSlug);
    }

    private static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email,
        string password,
        string firstName,
        string lastName,
        Guid tenantId,
        UserRole primaryRole,
        string roleName)
    {
        var existing = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            TenantId = tenantId,
            PrimaryRole = primaryRole,
            IsActive = true,
            CreatedOn = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to seed user {Email}: {Errors}", email, errors);
            return null;
        }

        await userManager.AddToRoleAsync(user, roleName).ConfigureAwait(false);
        return user;
    }
}
