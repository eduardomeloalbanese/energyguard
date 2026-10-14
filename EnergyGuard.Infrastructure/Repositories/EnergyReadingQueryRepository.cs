using EnergyGuard.Application.Interfaces;
using EnergyGuard.Domain.Entities;
using EnergyGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyGuard.Infrastructure.Repositories;

public class EnergyReadingQueryRepository : IEnergyReadingQueryRepository
{
    private readonly ApplicationDbContext _context;

    public EnergyReadingQueryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<EnergyReading> Items, int TotalCount)> GetByDevicePagedAsync(Guid deviceId, int page, int pageSize)
    {
        var query = _context.EnergyReadings.Where(r => r.DeviceId == deviceId);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReadingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<(IEnumerable<EnergyReading> Items, int TotalCount)> GetByDateRangePagedAsync(DateTime start, DateTime end, int page, int pageSize)
    {
        var query = _context.EnergyReadings.Where(r => r.ReadingDate >= start && r.ReadingDate <= end);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReadingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<EnergyReading?> GetLastByDeviceAsync(Guid deviceId) =>
        await _context.EnergyReadings
            .Where(r => r.DeviceId == deviceId)
            .OrderByDescending(r => r.ReadingDate)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<EnergyReading>> GetByDateRangeAsync(DateTime start, DateTime end) =>
        await _context.EnergyReadings
            .Where(r => r.ReadingDate >= start && r.ReadingDate <= end)
            .ToListAsync();
}
