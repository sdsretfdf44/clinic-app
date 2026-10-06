using System.Reflection;
using Arrapa.ClinicOS.Shared.Constants;
using Microsoft.OpenApi.Models;

namespace Arrapa.ClinicOS.API.Extensions;

/// <summary>
/// Wires up Swashbuckle with JWT bearer support and product-branded metadata.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddArrapaSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"{AppConstants.ApplicationName} API",
                Version = "v1",
                Description = "The Complete Clinic Operating System — REST API.",
                Contact = new OpenApiContact { Name = AppConstants.CompanyName, Url = new Uri("https://arrapa.software") }
            });

            var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            var jwtScheme = new OpenApiSecurityScheme
            {
                Name = AppConstants.Http.AuthorizationHeader,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT access token."
            };

            options.AddSecurityDefinition("Bearer", jwtScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });

            var tenantHeader = new OpenApiSecurityScheme
            {
                Name = AppConstants.Http.TenantHeader,
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Optional tenant discriminator (GUID or slug)."
            };
            options.AddSecurityDefinition("TenantHeader", tenantHeader);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "TenantHeader" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseArrapaSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{AppConstants.ApplicationName} v1");
            options.DocumentTitle = $"{AppConstants.ApplicationName} API";
            options.DefaultModelsExpandDepth(-1);
        });
        return app;
    }
}
