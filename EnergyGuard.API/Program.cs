using System.Text;
using EnergyGuard.API.HealthChecks;
using EnergyGuard.API.Middleware;
using EnergyGuard.Application.Interfaces;
using EnergyGuard.Application.Services;
using EnergyGuard.Application.Validators;
using EnergyGuard.Domain.Entities;
using EnergyGuard.Infrastructure.Data;
using EnergyGuard.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (using AddIdentityCore so JWT schemes are not overridden)
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IDeviceQueryRepository, DeviceQueryRepository>();
builder.Services.AddScoped<IEnergyReadingQueryRepository, EnergyReadingQueryRepository>();
builder.Services.AddScoped<IAlertQueryRepository, AlertQueryRepository>();

// Services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IEnergyReadingService, EnergyReadingService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IReportService, ReportService>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDeviceRequestValidator>();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Health checks
// "live"  -> a aplicacao esta de pe (liveness), sem dependencias externas.
// "ready" -> a aplicacao e suas dependencias essenciais (banco) estao prontas (readiness).
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running."), tags: new[] { "live" })
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EnergyGuard API - ESG Energy Efficiency",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Aplica as migrations automaticamente em provedores relacionais (ex.: SQL Server).
// Os testes de integracao usam o provedor InMemory (nao relacional) e sao ignorados aqui.
await ApplyMigrationsAsync(app);

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Liveness: a aplicacao respondeu (nao valida o banco).
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
});

// Readiness: a aplicacao esta pronta e o banco esta acessivel.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});

// Agregado (todos os checks).
app.MapHealthChecks("/health");

app.Run();

// Aplica as migrations com uma pequena politica de retry, util quando o container do
// banco ainda esta inicializando no primeiro start do docker-compose.
static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!db.Database.IsRelational())
        return; // Provedor InMemory (testes): nada a migrar.

    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup.Migrations");

    const int maxAttempts = 10;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            logger.LogInformation("Migrations aplicadas com sucesso.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex,
                "Falha ao aplicar migrations (tentativa {Attempt}/{Max}). Nova tentativa em 5s...",
                attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

public partial class Program { }
