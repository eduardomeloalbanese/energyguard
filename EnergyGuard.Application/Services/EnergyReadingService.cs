using EnergyGuard.Application.Interfaces;
using EnergyGuard.Application.ViewModels;
using EnergyGuard.Domain.Entities;

namespace EnergyGuard.Application.Services;

public class EnergyReadingService : IEnergyReadingService
{
    private readonly IRepository<EnergyReading> _repository;
    private readonly IEnergyReadingQueryRepository _queryRepository;
    private readonly IAlertService _alertService;

    public EnergyReadingService(
        IRepository<EnergyReading> repository,
        IEnergyReadingQueryRepository queryRepository,
        IAlertService alertService)
    {
        _repository = repository;
        _queryRepository = queryRepository;
        _alertService = alertService;
    }

    public async Task<PagedResult<EnergyReadingResponse>> GetByDeviceAsync(Guid deviceId, int page, int pageSize)
    {
        var (items, total) = await _queryRepository.GetByDevicePagedAsync(deviceId, page, pageSize);
        return new PagedResult<EnergyReadingResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<EnergyReadingResponse> RegisterReadingAsync(CreateEnergyReadingRequest request)
    {
        var reading = new EnergyReading
        {
            Id = Guid.NewGuid(),
            DeviceId = request.DeviceId,
            KilowattHour = request.KilowattHour,
            ReadingDate = request.ReadingDate,
            Location = request.Location,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(reading);
        await _repository.SaveChangesAsync();

        await _alertService.CheckAndGenerateAlertsAsync(request.DeviceId);

        return MapToResponse(reading);
    }

    public async Task<PagedResult<EnergyReadingResponse>> GetReadingsByDateRangeAsync(DateTime start, DateTime end, int page, int pageSize)
    {
        var (items, total) = await _queryRepository.GetByDateRangePagedAsync(start, end, page, pageSize);
        return new PagedResult<EnergyReadingResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    private static EnergyReadingResponse MapToResponse(EnergyReading r) => new()
    {
        Id = r.Id,
        DeviceId = r.DeviceId,
        KilowattHour = r.KilowattHour,
        ReadingDate = r.ReadingDate,
        Location = r.Location
    };
}
