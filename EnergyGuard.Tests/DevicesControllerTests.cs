using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace EnergyGuard.Tests;

public class DevicesControllerTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;
    private readonly TestFactory _factory;

    public DevicesControllerTests(TestFactory factory)
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
    public async Task GetAll_ReturnsHttpStatusCode200()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/devices");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsHttpStatusCode200()
    {
        await AuthenticateAsync();
        var response = await _client.PostAsJsonAsync("/api/devices", new
        {
            Name = "Device Test",
            SerialNumber = "SN-TEST-001",
            Location = "Building A",
            PowerLimitKwh = 100.0
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
