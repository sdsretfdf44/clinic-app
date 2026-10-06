namespace Arrapa.ClinicOS.IntegrationTests;

public sealed class SecurityHeadersTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public SecurityHeadersTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Responses_include_expected_security_headers()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

        var contentTypeOptions = TryGetHeader(response, "X-Content-Type-Options");
        contentTypeOptions.Should().Be("nosniff");

        TryGetHeader(response, "X-Frame-Options").Should().Be("DENY");
        TryGetHeader(response, "Referrer-Policy").Should().Be("strict-origin-when-cross-origin");
    }

    private static string? TryGetHeader(HttpResponseMessage response, string name)
    {
        if (response.Headers.TryGetValues(name, out var values))
        {
            return string.Join(", ", values);
        }

        if (response.Content.Headers.TryGetValues(name, out var contentValues))
        {
            return string.Join(", ", contentValues);
        }

        return null;
    }
}
