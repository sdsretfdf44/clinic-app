using System.Text;
using Arrapa.ClinicOS.Application.Abstractions.Email;
using Arrapa.ClinicOS.Application.Abstractions.FeatureFlags;
using Arrapa.ClinicOS.Application.Abstractions.Identity;
using Arrapa.ClinicOS.Application.Abstractions.MultiTenancy;
using Arrapa.ClinicOS.Application.Abstractions.Notifications;
using Arrapa.ClinicOS.Domain.Enums;
using Arrapa.ClinicOS.Infrastructure.Email;
using Arrapa.ClinicOS.Infrastructure.FeatureFlags;
using Arrapa.ClinicOS.Infrastructure.FileStorage;
using Arrapa.ClinicOS.Infrastructure.Identity;
using Arrapa.ClinicOS.Infrastructure.MultiTenancy;
using Arrapa.ClinicOS.Infrastructure.Notifications;
using Arrapa.ClinicOS.Infrastructure.Services;
using Arrapa.ClinicOS.Shared.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Arrapa.ClinicOS.Infrastructure;

/// <summary>
/// Registers infrastructure-layer services: identity abstractions, JWT authentication, tenant resolution, clock.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantContext, TenantContext>();

        AddJwtAuthentication(services, configuration);
        AddEmail(services, configuration);
        services.AddScoped<IFeatureFlagEvaluator, FeatureFlagEvaluator>();
        AddNotifications(services, configuration);
        services.AddArrapaFileStorage(configuration);

        services.AddAuthorization();

        return services;
    }

    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantResolutionMiddleware>();

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey) && o.SigningKey.Length >= 32, "JWT signing key must be at least 32 characters.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "JWT issuer must be configured.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "JWT audience must be configured.")
            .Validate(o => o.AccessTokenMinutes > 0, "JWT access token lifetime must be positive.")
            .Validate(o => o.RefreshTokenDays > 0, "JWT refresh token lifetime must be positive.")
            .ValidateOnStart();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var signingKey = jwtSection[nameof(JwtOptions.SigningKey)] ?? string.Empty;

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection[nameof(JwtOptions.Issuer)],
                    ValidAudience = jwtSection[nameof(JwtOptions.Audience)],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });
    }

    private static void AddEmail(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.FromEmail), "Email FromEmail must be configured.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.PasswordResetUrlTemplate), "Email PasswordResetUrlTemplate must be configured.");

        var provider = configuration.GetSection(EmailOptions.SectionName)[nameof(EmailOptions.Provider)] ?? "Log";
        if (string.Equals(provider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, LogEmailService>();
        }
    }

    private static void AddNotifications(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<NotificationsOptions>()
            .Bind(configuration.GetSection(NotificationsOptions.SectionName));

        services.AddHttpClient("twilio");

        var notificationsSection = configuration.GetSection(NotificationsOptions.SectionName);
        var whatsAppProvider = notificationsSection.GetSection(nameof(NotificationsOptions.WhatsApp))[nameof(WhatsAppProviderOptions.Provider)] ?? string.Empty;
        var smsProvider = notificationsSection.GetSection(nameof(NotificationsOptions.Sms))[nameof(SmsProviderOptions.Provider)] ?? string.Empty;

        if (string.Equals(whatsAppProvider, "Twilio", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<INotificationSender, TwilioWhatsAppSender>();
        }
        else
        {
            services.AddScoped<INotificationSender>(sp => new LogNotificationSender(
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LogNotificationSender>>(),
                NotificationChannel.WhatsApp));
        }

        if (string.Equals(smsProvider, "Twilio", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<INotificationSender, TwilioSmsSender>();
        }
        else
        {
            services.AddScoped<INotificationSender>(sp => new LogNotificationSender(
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LogNotificationSender>>(),
                NotificationChannel.Sms));
        }

        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        services.AddHostedService<AppointmentReminderService>();
    }
}
