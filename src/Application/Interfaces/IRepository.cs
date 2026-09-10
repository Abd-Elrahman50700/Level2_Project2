using System.Linq.Expressions;
using Domain.Common;

namespace Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    System.Threading.Tasks.Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    System.Threading.Tasks.Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
