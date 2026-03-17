namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IUnitOfWork
    {
        public Task BeginTransactionAsync();
        public Task CommitAsync();
        public Task RollbackAsync();
        public Task SaveChangesAsync();
    }
}
