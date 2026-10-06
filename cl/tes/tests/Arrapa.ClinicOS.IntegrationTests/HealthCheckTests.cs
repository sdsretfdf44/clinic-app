using System.Net;

namespace Arrapa.ClinicOS.IntegrationTests;

public sealed class HealthCheckTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public HealthCheckTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Liveness_endpoint_returns_ok()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
    }
}
