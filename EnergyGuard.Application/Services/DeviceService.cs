using EnergyGuard.Application.Exceptions;
using EnergyGuard.Application.Interfaces;
using EnergyGuard.Application.ViewModels;
using EnergyGuard.Domain.Entities;

namespace EnergyGuard.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IRepository<Device> _repository;
    private readonly IDeviceQueryRepository _queryRepository;

    public DeviceService(IRepository<Device> repository, IDeviceQueryRepository queryRepository)
    {
        _repository = repository;
        _queryRepository = queryRepository;
    }

    public async Task<PagedResult<DeviceResponse>> GetAllAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllPagedAsync(page, pageSize);
        return new PagedResult<DeviceResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<DeviceResponse?> GetByIdAsync(Guid id)
    {
        var device = await _repository.GetByIdAsync(id);
        return device is null ? null : MapToResponse(device);
    }

    public async Task<DeviceResponse> CreateAsync(CreateDeviceRequest request)
    {
        var exists = await _queryRepository.SerialNumberExistsAsync(request.SerialNumber);
        if (exists)
            throw new ValidationException($"SerialNumber '{request.SerialNumber}' already exists.");

        var device = new Device
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SerialNumber = request.SerialNumber,
            Location = request.Location,
            PowerLimitKwh = request.PowerLimitKwh,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(device);
        await _repository.SaveChangesAsync();
        return MapToResponse(device);
    }

    public async Task<DeviceResponse?> UpdateAsync(Guid id, UpdateDeviceRequest request)
    {
        var device = await _repository.GetByIdAsync(id);
        if (device is null) return null;

        device.Name = request.Name;
        device.Location = request.Location;
        device.PowerLimitKwh = request.PowerLimitKwh;
        device.IsActive = request.IsActive;

        _repository.Update(device);
        await _repository.SaveChangesAsync();
        return MapToResponse(device);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var device = await _repository.GetByIdAsync(id);
        if (device is null) return false;

        device.IsActive = false;
        _repository.Update(device);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static DeviceResponse MapToResponse(Device d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        SerialNumber = d.SerialNumber,
        Location = d.Location,
        IsActive = d.IsActive,
        PowerLimitKwh = d.PowerLimitKwh,
        CreatedAt = d.CreatedAt
    };
}
