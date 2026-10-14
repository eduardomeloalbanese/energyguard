using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EnergyGuard.Tests;

public class AuthControllerTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;
    private readonly TestFactory _factory;

    public AuthControllerTests(TestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsHttpStatusCode200()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "newuser1",
            Email = "newuser1@test.com",
            Password = "Test@123456"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsHttpStatusCode200()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "loginuser",
            Email = "loginuser@test.com",
            Password = "Test@123456"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "loginuser@test.com",
            Password = "Test@123456"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
