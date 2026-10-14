using EnergyGuard.Application.Interfaces;
using EnergyGuard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyGuard.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        var total = await _context.Set<T>().CountAsync();
        var items = await _context.Set<T>()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<T?> GetByIdAsync(Guid id) =>
        await _context.Set<T>().FindAsync(id);

    public async Task AddAsync(T entity) =>
        await _context.Set<T>().AddAsync(entity);

    public void Update(T entity) =>
        _context.Set<T>().Update(entity);

    public void Delete(T entity) =>
        _context.Set<T>().Remove(entity);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
