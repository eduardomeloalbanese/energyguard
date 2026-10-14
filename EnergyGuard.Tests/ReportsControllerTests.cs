using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace EnergyGuard.Tests;

public class ReportsControllerTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;
    private readonly TestFactory _factory;

    public ReportsControllerTests(TestFactory factory)
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
    public async Task GetReports_ReturnsHttpStatusCode200()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/reports");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GenerateReport_ReturnsHttpStatusCode200()
    {
        await AuthenticateAsync();
        var response = await _client.PostAsJsonAsync("/api/reports/generate", new
        {
            Title = "Monthly Report",
            PeriodStart = DateTime.UtcNow.AddDays(-30),
            PeriodEnd = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
