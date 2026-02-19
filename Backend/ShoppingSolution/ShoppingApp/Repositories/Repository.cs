using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.Repositories;

namespace ShoppingApp.Repositories
{
    public class Repository<K, C> : IRepository<K, C> where C : class
    {
        protected ShoppingContext _context;

        public Repository(ShoppingContext context)
        {
            _context = context;
        }

        public async Task<C?> Add(C item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<C?> Delete(K key)
        {
            var item = await Get(key);
            if (item != null)
            {
                _context.Remove(item);
                await _context.SaveChangesAsync();
                return item;
            }
            return null;
        }

        public async Task<IEnumerable<C>?> GetAll()
        {
            var items = await _context.Set<C>().ToListAsync();
            if (items.Any())
                return items;
            return null;
        }

        public async Task<C?> Get(K key)
        {
            var item = await _context.FindAsync<C>(key);
            return item != null ? item : null;
        }

        public async Task<C?> Update(K key, C item)
        {
            var existingItem = await Get(key);
            if (existingItem != null)
            {
                _context.Entry(existingItem).CurrentValues.SetValues(item);
                await _context.SaveChangesAsync();
                return existingItem;
            }
            return null;
        }
        }
    }
}
