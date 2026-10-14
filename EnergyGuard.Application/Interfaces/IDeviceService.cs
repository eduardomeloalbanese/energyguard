using EnergyGuard.Application.ViewModels;

namespace EnergyGuard.Application.Interfaces;

public interface IDeviceService
{
    Task<PagedResult<DeviceResponse>> GetAllAsync(int page, int pageSize);
    Task<DeviceResponse?> GetByIdAsync(Guid id);
    Task<DeviceResponse> CreateAsync(CreateDeviceRequest request);
    Task<DeviceResponse?> UpdateAsync(Guid id, UpdateDeviceRequest request);
    Task<bool> DeleteAsync(Guid id);
}
