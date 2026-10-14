using EnergyGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EnergyGuard.API.HealthChecks;

/// <summary>
/// Readiness check: verifica se o banco de dados esta acessivel.
/// Nao expoe connection strings nem detalhes sensiveis na resposta.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseHealthCheck(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Banco de dados acessivel.")
                : HealthCheckResult.Unhealthy("Banco de dados indisponivel.");
        }
        catch (Exception)
        {
            // Mensagem generica de proposito: nao vazar detalhes de infraestrutura.
            return HealthCheckResult.Unhealthy("Banco de dados indisponivel.");
        }
    }
}
