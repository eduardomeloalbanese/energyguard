namespace EnergyGuard.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<(IEnumerable<T> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}
