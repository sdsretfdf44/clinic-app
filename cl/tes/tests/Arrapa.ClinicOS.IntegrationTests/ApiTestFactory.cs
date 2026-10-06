using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Arrapa.ClinicOS.IntegrationTests;

/// <summary>
/// A <see cref="WebApplicationFactory{TEntryPoint}"/> that boots the API with a stubbed configuration
/// so the DI graph builds cleanly. Endpoints that do not touch the database (health, security headers,
/// auth-challenge) work as-is; DB-backed scenarios require a Testcontainers-based factory (see roadmap).
/// </summary>
public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Provider is unreachable — that's fine, the tests avoid endpoints that query it.
                ["ConnectionStrings:Default"] = "Server=127.0.0.1,65534;Database=arrapa_test;User=sa;Password=Arrapa_Test_1;TrustServerCertificate=True",
                ["Jwt:SigningKey"] = "integration-test-signing-key-not-for-production-abcdef0123456789",
                ["Jwt:Issuer"] = "arrapa.clinicos.api",
                ["Jwt:Audience"] = "arrapa.clinicos.web",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "30",
                ["Email:Provider"] = "Log",
                ["Email:FromEmail"] = "no-reply@arrapa.software",
                ["Email:PasswordResetUrlTemplate"] = "/reset-password?email={email}&token={token}&tenant={tenant}",
                ["Database:SeedOnStart"] = "false",
                ["FileStorage:Provider"] = "LocalDisk",
                ["FileStorage:LocalRootPath"] = Path.Combine(Path.GetTempPath(), $"arrapa-tests-{Guid.NewGuid():N}"),
                ["DataProtection:KeysDirectory"] = Path.Combine(Path.GetTempPath(), $"arrapa-tests-dpk-{Guid.NewGuid():N}"),
                ["Notifications:Enabled"] = "false",
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173",
            });
        });
    }
}
