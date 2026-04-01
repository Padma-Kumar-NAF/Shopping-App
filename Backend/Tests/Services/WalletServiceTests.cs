using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class WalletServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private WalletService GetService(ShoppingContext context)
        {
            return new WalletService(new Repository<Guid, Wallet>(context));
        }

        // ── GetWalletAmount ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetWalletAmount_WalletExists_ReturnsBalance()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Wallets.Add(new Wallet { WalletId = Guid.NewGuid(), UserId = userId, WalletAmount = 500 });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetWalletAmount(userId);

            Assert.Equal(500, result.Data.WalletBalance);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetWalletAmount_NoWallet_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.GetWalletAmount(Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetWalletAmount_ZeroBalance_ReturnsZero()
        {
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            context.Wallets.Add(new Wallet { WalletId = Guid.NewGuid(), UserId = userId, WalletAmount = 0 });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetWalletAmount(userId);

            Assert.Equal(0, result.Data.WalletBalance);
        }
    }
}
