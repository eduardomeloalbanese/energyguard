using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace EnergyGuard.Tests;

public class AlertsControllerTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;
    private readonly TestFactory _factory;

    public AlertsControllerTests(TestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var token = await _factory.GetAuthTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetActiveAlerts_ReturnsHttpStatusCode200()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/alerts/active");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
