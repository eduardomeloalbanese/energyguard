using EnergyGuard.Domain.Entities;

namespace EnergyGuard.Application.Interfaces;

public interface IEnergyReadingQueryRepository
{
    Task<(IEnumerable<EnergyReading> Items, int TotalCount)> GetByDevicePagedAsync(Guid deviceId, int page, int pageSize);
    Task<(IEnumerable<EnergyReading> Items, int TotalCount)> GetByDateRangePagedAsync(DateTime start, DateTime end, int page, int pageSize);
    Task<EnergyReading?> GetLastByDeviceAsync(Guid deviceId);
    Task<IEnumerable<EnergyReading>> GetByDateRangeAsync(DateTime start, DateTime end);
}
