using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Persistence;
using Arrapa.ClinicOS.Domain.Identity;
using Arrapa.ClinicOS.Persistence.Context;
using Arrapa.ClinicOS.Persistence.Interceptors;
using Arrapa.ClinicOS.Persistence.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arrapa.ClinicOS.Persistence;

/// <summary>
/// Registers EF Core, ASP.NET Identity, and persistence-side interceptors.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<AuditLogInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60);
            });

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
                sp.GetRequiredService<AuditLogInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());

            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddMemoryCache();
        services.AddScoped<ITenantLookupService, TenantLookupService>();
        services.AddScoped<Application.Abstractions.Patients.IPatientMrnGenerator, Services.PatientMrnGenerator>();
        services.AddScoped<Application.Abstractions.Appointments.IAppointmentNumberGenerator, Services.AppointmentNumberGenerator>();
        services.AddScoped<Application.Abstractions.Billing.IInvoiceNumberGenerator, Services.InvoiceNumberGenerator>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
