using System.Net;

namespace Arrapa.ClinicOS.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AuthEndpointsTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Me_endpoint_requires_authentication()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/v1/auth/me", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Change_password_endpoint_requires_authentication()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync(
            new Uri("/api/v1/auth/change-password", UriKind.Relative),
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
