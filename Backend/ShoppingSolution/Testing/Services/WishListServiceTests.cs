using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class WishListServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private WishListService GetService(ShoppingContext context)
        {
            return new WishListService(
                new Repository<Guid, WishList>(context),
                new Repository<Guid, WishListItems>(context),
                new Repository<Guid, Product>(context),
                new UnitOfWork(context)
            );
        }

        private async Task<(Guid userId, Guid productId)> SeedAsync(ShoppingContext context)
        {
            var userId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            var prodId = Guid.NewGuid();

            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Cat" });
            context.Products.Add(new Product { ProductId = prodId, CategoryId = catId, Name = "Widget", ImagePath = "img", Description = "desc", Price = 100, ActiveStatus = true });
            await context.SaveChangesAsync();

            return (userId, prodId);
        }

        // ── CreateWishList ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateWishList_Success_ReturnsIsCreated()
        {
            var context = GetDbContext();
            var (userId, _) = await SeedAsync(context);
            var service = GetService(context);

            var result = await service.CreateWishListAsync("Favourites", userId);
            Assert.True(result.Data.IsCreated);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task CreateWishList_Duplicate_ThrowsAppException()
        {
            var context = GetDbContext();
            var (userId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.CreateWishListAsync("Favourites", userId);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.CreateWishListAsync("Favourites", userId));
            Assert.NotNull(ex);
        }

        // ── AddToWishList ────────────────────────────────────────────────────────

        [Fact]
        public async Task AddToWishList_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            var created = await service.CreateWishListAsync("MyList", userId);
            var wishListId = context.WishList.First(w => w.UserId == userId).WishListId;

            var result = await service.AddToWishListAsync(userId, prodId, wishListId);
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task AddToWishList_WishListNotFound_ThrowsAppException()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddToWishListAsync(userId, prodId, Guid.NewGuid()));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task AddToWishList_DuplicateProduct_ThrowsAppException()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            await service.CreateWishListAsync("MyList", userId);
            var wishListId = context.WishList.First(w => w.UserId == userId).WishListId;

            await service.AddToWishListAsync(userId, prodId, wishListId);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddToWishListAsync(userId, prodId, wishListId));
            Assert.NotNull(ex);
        }

        // ── RemoveFromWishList ───────────────────────────────────────────────────

        [Fact]
        public async Task RemoveFromWishList_Success_ReturnsIsRemoved()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            await service.CreateWishListAsync("MyList", userId);
            var wishListId = context.WishList.First(w => w.UserId == userId).WishListId;
            await service.AddToWishListAsync(userId, prodId, wishListId);

            var result = await service.RemoveFromWishListAsync(userId, wishListId, prodId);
            Assert.True(result.Data.IsRemoved);
        }

        [Fact]
        public async Task RemoveFromWishList_ItemNotFound_ThrowsAppException()
        {
            var context = GetDbContext();
            var (userId, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.RemoveFromWishListAsync(userId, Guid.NewGuid(), Guid.NewGuid()));
            Assert.NotNull(ex);
        }

        // ── DeleteWishList ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteWishList_Success_ReturnsIsDeleted()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            await service.CreateWishListAsync("ToDelete", userId);
            var wishListId = context.WishList.First(w => w.UserId == userId).WishListId;
            await service.AddToWishListAsync(userId, prodId, wishListId);

            var result = await service.DeleteWishListAsync(userId, wishListId);
            Assert.True(result.Data.IsDeleted);
        }

        [Fact]
        public async Task DeleteWishList_NotFound_ThrowsAppException()
        {
            var context = GetDbContext();
            var (userId, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteWishListAsync(userId, Guid.NewGuid()));
            Assert.NotNull(ex);
        }

        // ── GetUserWishList ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetUserWishList_WithData_ReturnsList()
        {
            var context = GetDbContext();
            var (userId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.CreateWishListAsync("MyList", userId);
            var result = await service.GetUserWishListAsync(10, 1, userId);

            Assert.NotEmpty(result.Data.WishList);
        }

        [Fact]
        public async Task GetUserWishList_Empty_ReturnsEmptyMessage()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetUserWishListAsync(10, 1, Guid.NewGuid());
            Assert.Equal("No wishlist found", result.Message);
        }

        [Fact]
        public async Task GetUserWishList_InvalidPagination_DefaultsGracefully()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetUserWishListAsync(-1, -1, Guid.NewGuid());
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task AddToWishList_ProductNotFound_ThrowsAppException()
        {
            var context = GetDbContext();
            var (userId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.CreateWishListAsync("MyList", userId);
            var wishListId = context.WishList.First(w => w.UserId == userId).WishListId;

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddToWishListAsync(userId, Guid.NewGuid(), wishListId));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteWishList_WithNoItems_DeletesSuccessfully()
        {
            // Covers the branch where wishListItems.Any() is false
            var context = GetDbContext();
            var (userId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.CreateWishListAsync("EmptyList", userId);
            var wishListId = context.WishList.First(w => w.UserId == userId).WishListId;

            var result = await service.DeleteWishListAsync(userId, wishListId);
            Assert.True(result.Data.IsDeleted);
        }

        [Fact]
        public async Task GetUserWishList_WithItems_ReturnsItemDetails()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            await service.CreateWishListAsync("MyList", userId);
            var wishListId = context.WishList.First(w => w.UserId == userId).WishListId;
            await service.AddToWishListAsync(userId, prodId, wishListId);

            var result = await service.GetUserWishListAsync(10, 1, userId);
            Assert.Single(result.Data.WishList.First().WishListItems);
        }
    }
}
