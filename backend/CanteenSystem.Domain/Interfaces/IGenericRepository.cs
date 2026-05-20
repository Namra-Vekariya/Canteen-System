using System.Linq.Expressions;

namespace CanteenSystem.Domain.Interfaces;

public interface IGenericRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> SaveChangesAsync();
}