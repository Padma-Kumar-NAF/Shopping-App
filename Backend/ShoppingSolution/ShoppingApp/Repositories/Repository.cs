using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using System.Linq.Expressions;

namespace ShoppingApp.Repositories
{
    public class Repository<K, C> : IRepository<K, C> where C : class
    {
        protected ShoppingContext _context;

        public Repository(ShoppingContext context)
        {
            _context = context;
        }

        public async Task<C?> AddAsync(C item)
        {
            var user = _context.Add(item);
            await _context.SaveChangesAsync();
            if(user != null)
            {
                return item;
            }
            return null;
        }

        public async Task<C?> DeleteAsync(K key)
        {
            var item = await GetAsync(key);
            if (item != null)
            {
                _context.Remove(item);
                await _context.SaveChangesAsync();
                return item;
            }
            return null;
        }

        public async Task<IEnumerable<C>> GetAllAsync()
        {
            return await _context.Set<C>().ToListAsync();
        }

        public async Task<C?> GetAsync(K key)
        {
            var item = await _context.FindAsync<C>(key);
            return item != null ? item : null;
        }

        public async Task<C?> UpdateAsync(K key, C item)
        {
            var existingItem = await GetAsync(key);
            if (existingItem != null)
            {
                _context.Entry(existingItem).CurrentValues.SetValues(item);
                await _context.SaveChangesAsync();
                return existingItem;
            }
            return null;
        }

        //-------------------------------------------------------------------------//
        public async Task<C?> FirstOrDefaultAsync(Expression<Func<C, bool>> predicate)
        {
            return await _context.Set<C>().FirstOrDefaultAsync(predicate);
        }

        public IQueryable<C> GetQueryable()
        {
            return _context.Set<C>();
        }

        public async Task<IEnumerable<C>> GetAllByForeignKeyAsync(Expression<Func<C, bool>> predicate,
        int limit,
        int pageNumber)
        {
            return await _context.Set<C>()
                .Where(predicate)
                .Skip((pageNumber - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }
    }
}