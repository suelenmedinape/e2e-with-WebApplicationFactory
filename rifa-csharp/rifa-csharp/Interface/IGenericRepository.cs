using System.Linq.Expressions;

namespace rifa_csharp.Interface;

public interface IGenericRepository<T> where T : class
{
    Task<List<T>> ListAll();
    Task<T?> findById(int id);
    void Add(T entity);
    void Update(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task<List<T>> findAsync(Expression<Func<T, bool>> predicate);
    void Delete(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountDistinctAsync<TKey>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TKey>> keySelector
    );
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes
    );
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> Query();
}