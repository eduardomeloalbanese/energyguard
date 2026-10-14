using EnergyGuard.Application.ViewModels;

namespace EnergyGuard.Application.Interfaces;

public interface IEnergyReadingService
{
    Task<PagedResult<EnergyReadingResponse>> GetByDeviceAsync(Guid deviceId, int page, int pageSize);
    Task<EnergyReadingResponse> RegisterReadingAsync(CreateEnergyReadingRequest request);
    Task<PagedResult<EnergyReadingResponse>> GetReadingsByDateRangeAsync(DateTime start, DateTime end, int page, int pageSize);
}
