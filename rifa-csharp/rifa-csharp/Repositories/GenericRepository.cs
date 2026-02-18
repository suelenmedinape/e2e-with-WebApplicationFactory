using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using rifa_csharp.Data;
using rifa_csharp.DTO.Ticket;
using rifa_csharp.Interface;

namespace rifa_csharp.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    public readonly AppDbContext _context;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<T>> ListAll()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T?> findById(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    
    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes
    )
    {
        var query = includes.Aggregate(
            _context.Set<T>().AsQueryable(),
            (current, include) => current.Include(include)
        );

        return await query.FirstOrDefaultAsync(predicate);
    }

    /*public async Task<T?> getAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(predicate);
    }*/
    
    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }
    
    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }
    
    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _context.Set<T>().AddRangeAsync(entities);
    }
    
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().AnyAsync(predicate);
    }

    public async Task<List<T>> findAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync();
    }
    
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().CountAsync(predicate);
    }

    public async Task<int> CountDistinctAsync<TKey>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TKey>> keySelector)
    {
        return await _context.Set<T>()
            .Where(predicate)
            .Select(keySelector)
            .Distinct()
            .CountAsync();
    }
    
    public IQueryable<T> Query()
    {
        return _context.Set<T>().AsQueryable();
    }
}