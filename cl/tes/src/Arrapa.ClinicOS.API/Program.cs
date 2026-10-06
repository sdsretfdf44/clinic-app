using Arrapa.ClinicOS.API.Extensions;
using Arrapa.ClinicOS.API.Middleware;
using Arrapa.ClinicOS.Application;
using Arrapa.ClinicOS.Infrastructure;
using Arrapa.ClinicOS.Persistence;
using Arrapa.ClinicOS.Persistence.Seeding;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables(prefix: "ARRAPA_")
    .AddEnvironmentVariables();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddArrapaCors(builder.Configuration);
builder.Services.AddArrapaApiVersioning();
builder.Services.AddArrapaSwagger();
builder.Services.AddArrapaHealthChecks(builder.Configuration);
builder.Services.AddArrapaRateLimiting(builder.Configuration);
builder.Services.AddArrapaDataProtection(builder.Configuration, builder.Environment);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    var maxUploadBytes = builder.Configuration.GetValue<long?>("FileStorage:MaxUploadBytes") ?? 10 * 1024 * 1024;
    options.MultipartBodyLengthLimit = Math.Max(maxUploadBytes, 10 * 1024 * 1024);
});

builder.Services.AddProblemDetails();

var enforceHttps = builder.Environment.IsProduction() || builder.Environment.IsStaging();
if (enforceHttps)
{
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });

    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    });
}

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseArrapaSwagger();
}

if (enforceHttps)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(CorsExtensions.PolicyName);

app.UseRateLimiter();

app.UseAuthentication();
app.UseTenantResolution();
app.UseAuthorization();

app.MapControllers();
app.MapArrapaHealthChecks();

if (app.Configuration.GetValue<bool>("Database:SeedOnStart"))
{
    await DbInitializer.InitializeAsync(app.Services);
}

app.Run();

// Public partial declaration required by WebApplicationFactory used by integration tests.
public partial class Program;
