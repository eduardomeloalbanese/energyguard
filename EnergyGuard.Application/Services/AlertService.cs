using EnergyGuard.Application.Interfaces;
using EnergyGuard.Application.ViewModels;
using EnergyGuard.Domain.Entities;
using EnergyGuard.Domain.Enums;

namespace EnergyGuard.Application.Services;

public class AlertService : IAlertService
{
    private readonly IRepository<Alert> _repository;
    private readonly IAlertQueryRepository _queryRepository;
    private readonly IRepository<Device> _deviceRepository;
    private readonly IEnergyReadingQueryRepository _readingQueryRepository;

    public AlertService(
        IRepository<Alert> repository,
        IAlertQueryRepository queryRepository,
        IRepository<Device> deviceRepository,
        IEnergyReadingQueryRepository readingQueryRepository)
    {
        _repository = repository;
        _queryRepository = queryRepository;
        _deviceRepository = deviceRepository;
        _readingQueryRepository = readingQueryRepository;
    }

    public async Task<PagedResult<AlertResponse>> GetActiveAlertsAsync(int page, int pageSize)
    {
        var (items, total) = await _queryRepository.GetActivePagedAsync(page, pageSize);
        return new PagedResult<AlertResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<AlertResponse?> ResolveAlertAsync(Guid alertId)
    {
        var alert = await _repository.GetByIdAsync(alertId);
        if (alert is null) return null;

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        _repository.Update(alert);
        await _repository.SaveChangesAsync();
        return MapToResponse(alert);
    }

    public async Task CheckAndGenerateAlertsAsync(Guid deviceId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device is null) return;

        var lastReading = await _readingQueryRepository.GetLastByDeviceAsync(deviceId);
        if (lastReading is null) return;

        if (lastReading.KilowattHour > device.PowerLimitKwh)
        {
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                DeviceId = deviceId,
                AlertType = AlertType.OverLimit,
                Message = $"Consumo {lastReading.KilowattHour} kWh ultrapassou limite de {device.PowerLimitKwh} kWh",
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(alert);
            await _repository.SaveChangesAsync();
        }
    }

    private static AlertResponse MapToResponse(Alert a) => new()
    {
        Id = a.Id,
        DeviceId = a.DeviceId,
        Message = a.Message,
        AlertType = a.AlertType,
        IsResolved = a.IsResolved,
        CreatedAt = a.CreatedAt
    };
}
