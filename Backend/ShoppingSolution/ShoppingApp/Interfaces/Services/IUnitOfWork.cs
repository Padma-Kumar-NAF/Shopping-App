namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IUnitOfWork
    {
        public Task BeginTransactionAsync();
        public Task<int> CommitAsync();
        public Task RollbackAsync();
        public Task<int> SaveChangesAsync();
    }
}
