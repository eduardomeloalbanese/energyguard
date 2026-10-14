using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace EnergyGuard.Tests;

public class EnergyReadingsControllerTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;
    private readonly TestFactory _factory;

    public EnergyReadingsControllerTests(TestFactory factory)
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
    public async Task GetByDateRange_ReturnsHttpStatusCode200()
    {
        await AuthenticateAsync();
        var start = DateTime.UtcNow.AddDays(-30).ToString("o");
        var end = DateTime.UtcNow.ToString("o");
        var response = await _client.GetAsync($"/api/energyreadings/range?start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterReading_ReturnsHttpStatusCode200()
    {
        await AuthenticateAsync();

        // Create a device first
        var deviceResponse = await _client.PostAsJsonAsync("/api/devices", new
        {
            Name = "Reading Device",
            SerialNumber = "SN-READING-001",
            Location = "Lab",
            PowerLimitKwh = 50.0
        });
        var device = await deviceResponse.Content.ReadFromJsonAsync<DeviceDto>();

        var response = await _client.PostAsJsonAsync("/api/energyreadings", new
        {
            DeviceId = device!.Id,
            KilowattHour = 10.5,
            ReadingDate = DateTime.UtcNow.AddMinutes(-1),
            Location = "Lab"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private class DeviceDto { public Guid Id { get; set; } }
}
