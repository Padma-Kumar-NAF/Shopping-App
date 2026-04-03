using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class UnitOfWorkTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(
                    Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        [Fact]
        public async Task BeginTransactionAsync_And_CommitAsync_Succeeds()
        {
            var context = GetDbContext();
            var uow = new UnitOfWork(context);

            await uow.BeginTransactionAsync();
            var result = await uow.CommitAsync();

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task RollbackAsync_WithActiveTransaction_Succeeds()
        {
            var context = GetDbContext();
            var uow = new UnitOfWork(context);

            await uow.BeginTransactionAsync();
            // Should not throw
            await uow.RollbackAsync();
        }

        [Fact]
        public async Task RollbackAsync_WithNoTransaction_DoesNotThrow()
        {
            var context = GetDbContext();
            var uow = new UnitOfWork(context);

            // No BeginTransaction called — should be a no-op
            await uow.RollbackAsync();
        }

        [Fact]
        public async Task CommitAsync_WithoutBeginTransaction_ThrowsInvalidOperation()
        {
            var context = GetDbContext();
            var uow = new UnitOfWork(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync());
        }

        [Fact]
        public async Task SaveChangesAsync_ReturnsAffectedRows()
        {
            var context = GetDbContext();
            var uow = new UnitOfWork(context);

            // No changes — should return 0
            var result = await uow.SaveChangesAsync();
            Assert.Equal(0, result);
        }
    }
}
