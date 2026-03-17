using Microsoft.EntityFrameworkCore.Storage;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.ServicesInterface;

namespace ShoppingApp.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ShoppingContext _context;
        private IDbContextTransaction? _transaction;
        public UnitOfWork(ShoppingContext context)
        {
            _context = context;
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
            await _transaction!.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await _transaction!.RollbackAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
