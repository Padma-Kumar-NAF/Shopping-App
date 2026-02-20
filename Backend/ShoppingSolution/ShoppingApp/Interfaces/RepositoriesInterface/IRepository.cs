using System.Linq.Expressions;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface IRepository<K,C> where C : class
    {
        Task<IEnumerable<C>> GetAllAsync();
        Task<C?> GetAsync(K key);
        Task<C?> AddAsync(C item);
        Task<C?> DeleteAsync(K key);
        Task<C?> UpdateAsync(K key, C entity);
        Task<C?> FirstOrDefaultAsync(Expression<Func<C, bool>> predicate);
        IQueryable<C> GetQueryable();
    }
}
