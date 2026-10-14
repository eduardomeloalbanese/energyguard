using EnergyGuard.Domain.Entities;

namespace EnergyGuard.Application.Interfaces;

public interface IAlertQueryRepository
{
    Task<(IEnumerable<Alert> Items, int TotalCount)> GetActivePagedAsync(int page, int pageSize);
}
