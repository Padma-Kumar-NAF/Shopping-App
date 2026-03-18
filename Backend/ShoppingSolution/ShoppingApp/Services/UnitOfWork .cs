using Microsoft.EntityFrameworkCore.Storage;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
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

        public async Task<int> CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started.");

            try
            {
                await _transaction.CommitAsync();
                return 1;
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await DisposeTransactionAsync();
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}