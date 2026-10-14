using EnergyGuard.Application.Interfaces;
using EnergyGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyGuard.Infrastructure.Repositories;

public class DeviceQueryRepository : IDeviceQueryRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceQueryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SerialNumberExistsAsync(string serialNumber) =>
        await _context.Devices.AnyAsync(d => d.SerialNumber == serialNumber);
}
