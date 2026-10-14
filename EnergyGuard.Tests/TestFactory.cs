using System.Net.Http.Json;
using EnergyGuard.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyGuard.Tests;

public class TestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "EnergyGuardTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "EnergyGuardSuperSecretKey2024XYZ!@#",
                ["Jwt:Issuer"] = "EnergyGuard.API",
                ["Jwt:Audience"] = "EnergyGuard.Client",
                ["Jwt:ExpirationHours"] = "8"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    public async Task<string> GetAuthTokenAsync(HttpClient client)
    {
        var email = $"testuser_{Guid.NewGuid():N}@energyguard.com";
        var password = "Test@123456";

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "testuser_" + Guid.NewGuid().ToString("N"),
            Email = email,
            Password = password
        });

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        if (!login.IsSuccessStatusCode)
            return string.Empty;

        var content = await login.Content.ReadFromJsonAsync<LoginResponseDto>();
        return content?.Token ?? string.Empty;
    }

    private class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
