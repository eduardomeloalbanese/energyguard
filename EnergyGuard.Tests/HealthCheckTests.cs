using System.Net;
using Xunit;

namespace EnergyGuard.Tests;

public class HealthCheckTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(TestFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Liveness_IsPublic_AndReturns200()
    {
        var response = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Readiness_WithAvailableDatabase_Returns200()
    {
        // No ambiente de teste o provedor InMemory responde como acessivel.
        var response = await _client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_IsReachableWithoutAuthentication()
    {
        // Endpoint de saude nao deve exigir token JWT.
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
