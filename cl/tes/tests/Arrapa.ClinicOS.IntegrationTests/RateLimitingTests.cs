using System.Net;
using System.Text;

namespace Arrapa.ClinicOS.IntegrationTests;

public sealed class RateLimitingTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public RateLimitingTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Auth_endpoints_are_rate_limited()
    {
        var client = _factory.CreateClient();

        var seenStatuses = new List<HttpStatusCode>();
        for (var attempt = 0; attempt < 15; attempt++)
        {
            var response = await client.PostAsync(
                new Uri("/api/v1/auth/forgot-password", UriKind.Relative),
                new StringContent("{\"email\":\"nobody@example.com\",\"tenantSlug\":\"arrapa\"}", Encoding.UTF8, "application/json"));
            seenStatuses.Add(response.StatusCode);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                break;
            }
        }

        seenStatuses.Should().Contain(HttpStatusCode.TooManyRequests,
            because: "the auth policy caps requests per client per minute");
    }
}
