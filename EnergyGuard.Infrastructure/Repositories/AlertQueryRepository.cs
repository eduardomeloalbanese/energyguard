using EnergyGuard.Application.Interfaces;
using EnergyGuard.Domain.Entities;
using EnergyGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyGuard.Infrastructure.Repositories;

public class AlertQueryRepository : IAlertQueryRepository
{
    private readonly ApplicationDbContext _context;

    public AlertQueryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Alert> Items, int TotalCount)> GetActivePagedAsync(int page, int pageSize)
    {
        var query = _context.Alerts.Where(a => !a.IsResolved);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }
}
