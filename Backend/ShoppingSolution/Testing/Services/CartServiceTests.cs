using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class CartServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private CartService GetService(ShoppingContext context)
        {
            return new CartService(
                new Repository<Guid, Cart>(context),
                new Repository<Guid, CartItem>(context),
                new Repository<Guid, Product>(context),
                new Repository<Guid, Stock>(context),
                new Repository<Guid, Order>(context),
                new Repository<Guid, Payment>(context),
                new Repository<Guid, Address>(context),
                new Repository<Guid, PromoCode>(context),
                new Repository<Guid, Wallet>(context),
                new UnitOfWork(context)
            );
        }

        private async Task<(Guid userId, Guid productId, Guid addressId)> SeedAsync(ShoppingContext context, int stock = 10)
        {
            var userId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            var prodId = Guid.NewGuid();
            var addrId = Guid.NewGuid();

            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Cat" });
            context.Products.Add(new Product { ProductId = prodId, CategoryId = catId, Name = "Widget", ImagePath = "img", Description = "desc", Price = 100, ActiveStatus = true });
            context.Stock.Add(new Stock { StockId = Guid.NewGuid(), ProductId = prodId, Quantity = stock });
            context.Addresses.Add(new Address { AddressId = addrId, UserId = userId, AddressLine1 = "A", AddressLine2 = "B", State = "S", City = "C", Pincode = "123456" });
            await context.SaveChangesAsync();

            return (userId, prodId, addrId);
        }

        // ── AddCart ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddCart_Success_ReturnsCartId()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            var result = await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 2 });

            Assert.NotEqual(Guid.Empty, result.Data.CartId);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task AddCart_ZeroQuantity_Throws400()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 0 }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task AddCart_ProductNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddCart(Guid.NewGuid(), new AddToCartRequestDTO { ProductId = Guid.NewGuid(), Quantity = 1 }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AddCart_ExistingItem_UpdatesQuantityIfHigher()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 2 });
            var result = await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 5 });

            Assert.Equal(200, result.StatusCode);
            Assert.Single(context.CartItems);
            Assert.Equal(5, context.CartItems.First().Quantity);
        }

        [Fact]
        public async Task AddCart_ExistingItem_DoesNotReduceQuantity()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 5 });
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 2 });

            Assert.Equal(5, context.CartItems.First().Quantity);
        }

        // ── GetUserCarts ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetUserCarts_NoCart_ReturnsCartNotFound()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetUserCarts(Guid.NewGuid(), 1, 10);
            Assert.Equal("Cart not found", result.Message);
        }

        [Fact]
        public async Task GetUserCarts_WithItems_ReturnsItems()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 3 });
            var result = await service.GetUserCarts(userId, 1, 10);

            Assert.NotEmpty(result.Data.CartItems);
        }

        // ── RemoveFromCart ───────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveFromCart_Success_ReturnsIsRemoved()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            var addResult = await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var cartId = addResult.Data.CartId;
            var cartItemId = addResult.Data.CartItemId;

            var result = await service.RemoveFromCart(userId, cartId, cartItemId, prodId);
            Assert.True(result.Data.IsRemoved);
        }

        [Fact]
        public async Task RemoveFromCart_ItemNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.RemoveFromCart(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task RemoveFromCart_ProductMismatch_Throws400()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            var addResult = await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var cartId = addResult.Data.CartId;
            var cartItemId = addResult.Data.CartItemId;

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.RemoveFromCart(userId, cartId, cartItemId, Guid.NewGuid()));
            Assert.Equal(400, ex.StatusCode);
        }

        // ── RemoveAllFromCart ────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveAllFromCart_Success_ReturnsIsRemoved()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var result = await service.RemoveAllFromCartByUserID(userId);

            Assert.True(result.Data.IsRemoved);
        }

        [Fact]
        public async Task RemoveAllFromCart_NoCart_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.RemoveAllFromCartByUserID(Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        // ── UpdateCart ───────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateCart_Success_ReturnsIsUpdated()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            var addResult = await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var cartId = addResult.Data.CartId;
            var cartItemId = addResult.Data.CartItemId;

            var result = await service.UpdateCart(userId, cartId, cartItemId, prodId, 5);
            Assert.True(result.Data.IsUpdated);
        }

        [Fact]
        public async Task UpdateCart_CartNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateCart(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCart_ZeroQuantity_Throws400()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            var addResult = await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var cartId = addResult.Data.CartId;
            var cartItemId = addResult.Data.CartItemId;

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateCart(userId, cartId, cartItemId, prodId, 0));
            Assert.Equal(400, ex.StatusCode);
        }

        // ── PlaceOrderAllFromCart ────────────────────────────────────────────────

        [Fact]
        public async Task PlaceOrderAllFromCart_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 2 });
            var result = await service.PlaceOrderAllFromCart(userId, addrId, "COD", "", false);

            Assert.True(result.Data.IsSuccess);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_NoCart_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrderAllFromCart(Guid.NewGuid(), Guid.NewGuid(), "COD", "", false));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_AddressNotFound_Throws404()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrderAllFromCart(userId, Guid.NewGuid(), "COD", "", false));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_InsufficientStock_Throws409()
        {
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context, stock: 1);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 5 });
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrderAllFromCart(userId, addrId, "COD", "", false));
            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_WithWallet_DeductsWalletAmount()
        {
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context);
            context.Wallets.Add(new Wallet { WalletId = Guid.NewGuid(), UserId = userId, WalletAmount = 10000 });
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var result = await service.PlaceOrderAllFromCart(userId, addrId, "Wallet", "", true);

            Assert.True(result.Data.WalletUsed > 0);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_WithWallet_PartialBalance_UsesAvailableOnly()
        {
            // Wallet has less than total — partial wallet payment
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context, stock: 10);
            context.Wallets.Add(new Wallet { WalletId = Guid.NewGuid(), UserId = userId, WalletAmount = 10 });
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var result = await service.PlaceOrderAllFromCart(userId, addrId, "Partial", "", true);

            Assert.Equal(10, result.Data.WalletUsed);
            Assert.Equal("Partial Wallet", result.Data.PaymentStatus);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_WithWallet_NoWallet_Throws404()
        {
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrderAllFromCart(userId, addrId, "Wallet", "", true));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_WithValidPromoCode_AppliesDiscount()
        {
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context);
            context.PromoCodes.Add(new PromoCode
            {
                PromoCodeId = Guid.NewGuid(),
                PromoCodeName = "CART10",
                DiscountPercentage = 10,
                FromDate = DateTime.UtcNow.AddDays(-1),
                ToDate = DateTime.UtcNow.AddDays(1)
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var result = await service.PlaceOrderAllFromCart(userId, addrId, "COD", "CART10", false);

            Assert.True(result.Data.DiscountAmount > 0);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_InvalidPromoCode_Throws400()
        {
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrderAllFromCart(userId, addrId, "COD", "BADCODE", false));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_ExpiredPromoCode_Throws400()
        {
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context);
            context.PromoCodes.Add(new PromoCode
            {
                PromoCodeId = Guid.NewGuid(),
                PromoCodeName = "EXPIRED",
                DiscountPercentage = 10,
                FromDate = DateTime.UtcNow.AddDays(-10),
                ToDate = DateTime.UtcNow.AddDays(-1)
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrderAllFromCart(userId, addrId, "COD", "EXPIRED", false));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_FuturePromoCode_Throws400()
        {
            var context = GetDbContext();
            var (userId, prodId, addrId) = await SeedAsync(context);
            context.PromoCodes.Add(new PromoCode
            {
                PromoCodeId = Guid.NewGuid(),
                PromoCodeName = "FUTURE",
                DiscountPercentage = 10,
                FromDate = DateTime.UtcNow.AddDays(5),
                ToDate = DateTime.UtcNow.AddDays(10)
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrderAllFromCart(userId, addrId, "COD", "FUTURE", false));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_FreeShipping_WhenSubtotalOver5000()
        {
            // Price=6000, qty=1 → subtotal > 5000 → shipping = 0
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            var prodId = Guid.NewGuid();
            var addrId = Guid.NewGuid();

            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Cat" });
            context.Products.Add(new Product { ProductId = prodId, CategoryId = catId, Name = "Expensive", ImagePath = "img", Description = "desc", Price = 6000, ActiveStatus = true });
            context.Stock.Add(new Stock { StockId = Guid.NewGuid(), ProductId = prodId, Quantity = 10 });
            context.Addresses.Add(new Address { AddressId = addrId, UserId = userId, AddressLine1 = "A", AddressLine2 = "B", State = "S", City = "C", Pincode = "123456" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var result = await service.PlaceOrderAllFromCart(userId, addrId, "COD", "", false);

            Assert.Equal(0, result.Data.Shipping);
        }

        [Fact]
        public async Task PlaceOrderAllFromCart_StockNotFound_Throws404()
        {
            // Product exists but no stock record
            var context = GetDbContext();
            var userId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            var prodId = Guid.NewGuid();
            var addrId = Guid.NewGuid();

            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Cat" });
            context.Products.Add(new Product { ProductId = prodId, CategoryId = catId, Name = "NoStock", ImagePath = "img", Description = "desc", Price = 100, ActiveStatus = true });
            // No stock added
            context.Addresses.Add(new Address { AddressId = addrId, UserId = userId, AddressLine1 = "A", AddressLine2 = "B", State = "S", City = "C", Pincode = "123456" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrderAllFromCart(userId, addrId, "COD", "", false));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetUserCarts_EmptyCart_ReturnsEmptyCartMessage()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            // Create cart but no items
            await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var cartId = context.Carts.First(c => c.UserId == userId).CartId;
            // Remove the item directly
            context.CartItems.RemoveRange(context.CartItems.Where(ci => ci.CartId == cartId));
            await context.SaveChangesAsync();

            var result = await service.GetUserCarts(userId, 1, 10);
            Assert.Equal("Cart is empty", result.Message);
        }

        [Fact]
        public async Task RemoveAllFromCart_NoItems_Throws404()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            // Create cart but no items
            context.Carts.Add(new Cart { CartId = Guid.NewGuid(), UserId = userId });
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.RemoveAllFromCartByUserID(userId));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCart_CartItemNotFound_Throws404()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            var addResult = await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var cartId = addResult.Data.CartId;

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateCart(userId, cartId, Guid.NewGuid(), prodId, 5));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateCart_ProductMismatch_Throws400()
        {
            var context = GetDbContext();
            var (userId, prodId, _) = await SeedAsync(context);
            var service = GetService(context);

            var addResult = await service.AddCart(userId, new AddToCartRequestDTO { ProductId = prodId, Quantity = 1 });
            var cartId = addResult.Data.CartId;
            var cartItemId = addResult.Data.CartItemId;

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateCart(userId, cartId, cartItemId, Guid.NewGuid(), 5));
            Assert.Equal(400, ex.StatusCode);
        }
    }
}
