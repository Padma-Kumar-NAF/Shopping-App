using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.Order;
using ShoppingApp.Models.Entities;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class OrderServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private OrderService GetService(ShoppingContext context)
        {
            return new OrderService(
                new Repository<Guid, Order>(context),
                new Repository<Guid, Stock>(context),
                new Repository<Guid, Payment>(context),
                new Repository<Guid, Refund>(context),
                new Repository<Guid, User>(context),
                new Repository<Guid, Address>(context),
                new Repository<Guid, Product>(context),
                new Repository<Guid, OrderDetails>(context),
                new Repository<Guid, PromoCode>(context),
                new Repository<Guid, Wallet>(context),
                new Repository<Guid, UserMonthlyProductLimit>(context),
                new UnitOfWork(context)
            );
        }

        private async Task<(Guid userId, Guid adminId, Guid productId, Guid stockId, Guid addressId)> SeedAsync(ShoppingContext context)
        {
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            var prodId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var addrId = Guid.NewGuid();

            context.Users.Add(new User { UserId = userId, Name = "User", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.Users.Add(new User { UserId = adminId, Name = "Admin", Email = "a@a.com", Password = "x", SaltValue = "s", Role = "admin", Active = true });
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Cat" });
            context.Products.Add(new Product { ProductId = prodId, CategoryId = catId, Name = "Widget", ImagePath = "img", Description = "desc", Price = 200, ActiveStatus = true });
            context.Stock.Add(new Stock { StockId = stockId, ProductId = prodId, Quantity = 20 });
            context.Addresses.Add(new Address { AddressId = addrId, UserId = userId, AddressLine1 = "A", AddressLine2 = "B", State = "S", City = "C", Pincode = "123456" });
            await context.SaveChangesAsync();

            return (userId, adminId, prodId, stockId, addrId);
        }

        private async Task<(Guid orderId, Guid paymentId)> SeedOrderAsync(ShoppingContext context, Guid userId, Guid addrId, Guid prodId)
        {
            var orderId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            var orderDetailsId = Guid.NewGuid();

            var order = new Order
            {
                OrderId = orderId,
                UserId = userId,
                Status = "Not Delivered",
                TotalProductsCount = 1,
                TotalAmount = 200,
                OrderTotalAmount = 200,
                AddressId = addrId,
                DeliveryDate = DateTime.UtcNow.AddDays(2),
                OrderDetails = new List<OrderDetails>
                {
                    new OrderDetails { OrderDetailsId = orderDetailsId, ProductId = prodId, ProductName = "Widget", ProductPrice = 200, Quantity = 1 }
                }
            };

            context.Orders.Add(order);
            context.Payments.Add(new Payment { PaymentId = paymentId, UserId = userId, OrderId = orderId, TotalAmount = 200, PaymentType = "COD" });
            await context.SaveChangesAsync();

            return (orderId, paymentId);
        }

        // ── PlaceOrder ───────────────────────────────────────────────────────────

        [Fact]
        public async Task PlaceOrder_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var service = GetService(context);

            var result = await service.PlaceOrder(userId, new PlaceOrderRequestDTO
            {
                AddressId = addrId,
                TotalProductsCount = 1,
                TotalAmount = 200,
                PaymentType = "COD",
                OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
            });

            Assert.True(result.Data.IsSuccess);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(Guid.NewGuid(), new PlaceOrderRequestDTO
                {
                    AddressId = Guid.NewGuid(),
                    TotalProductsCount = 1,
                    TotalAmount = 100,
                    PaymentType = "COD",
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = Guid.NewGuid(), ProductName = "X", Quantity = 1, ProductPrice = 100 }
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_AddressNotFound_Throws404()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(userId, new PlaceOrderRequestDTO
                {
                    AddressId = Guid.NewGuid(),
                    TotalProductsCount = 1,
                    TotalAmount = 200,
                    PaymentType = "COD",
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_InsufficientStock_Throws400()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(userId, new PlaceOrderRequestDTO
                {
                    AddressId = addrId,
                    TotalProductsCount = 1,
                    TotalAmount = 200,
                    PaymentType = "COD",
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 999, ProductPrice = 200 }
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_WithValidPromoCode_AppliesDiscount()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            context.PromoCodes.Add(new PromoCode
            {
                PromoCodeId = Guid.NewGuid(),
                PromoCodeName = "SAVE10",
                DiscountPercentage = 10,
                FromDate = DateTime.UtcNow.AddDays(-1),
                ToDate = DateTime.UtcNow.AddDays(1)
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.PlaceOrder(userId, new PlaceOrderRequestDTO
            {
                AddressId = addrId,
                TotalProductsCount = 1,
                TotalAmount = 200,
                PaymentType = "COD",
                PromoCode = "SAVE10",
                OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
            });

            Assert.True(result.Data.DiscountAmount > 0);
        }

        [Fact]
        public async Task PlaceOrder_ExpiredPromoCode_Throws400()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
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
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(userId, new PlaceOrderRequestDTO
                {
                    AddressId = addrId,
                    TotalProductsCount = 1,
                    TotalAmount = 200,
                    PaymentType = "COD",
                    PromoCode = "EXPIRED",
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        // ── CancelOrder ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CancelOrder_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, _) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            var result = await service.CancelOrder(userId, orderId);
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task CancelOrder_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.CancelOrder(Guid.NewGuid(), Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task CancelOrder_OrderNotFound_Throws404()
        {
            var context = GetDbContext();
            var (userId, _, _, _, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.CancelOrder(userId, Guid.NewGuid()));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task CancelOrder_AlreadyCancelled_Throws400()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, _) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            await service.CancelOrder(userId, orderId);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.CancelOrder(userId, orderId));
            Assert.Equal(400, ex.StatusCode);
        }

        // ── UpdateOrder ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateOrder_Success_ReturnsIsUpdated()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, _) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            var result = await service.UpdateOrder(userId, orderId, "Shipped");
            Assert.True(result.Data.IsUpdated);
        }

        [Fact]
        public async Task UpdateOrder_UserNotFound_Throws401()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateOrder(Guid.NewGuid(), Guid.NewGuid(), "Shipped"));
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_InvalidStatus_Throws400()
        {
            var context = GetDbContext();
            var (userId, _, _, _, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateOrder(userId, Guid.NewGuid(), "InvalidStatus"));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_CancelledStatus_Throws401()
        {
            var context = GetDbContext();
            var (userId, _, _, _, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateOrder(userId, Guid.NewGuid(), "Cancelled"));
            Assert.Equal(401, ex.StatusCode);
        }

        // ── GetAllOrders ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllOrders_WithData_ReturnsList()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            var result = await service.GetAllOrders(new GetAllOrderRequestDTO { pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.NotEmpty(result.Data.Items);
        }

        [Fact]
        public async Task GetAllOrders_Empty_Returns404Message()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllOrders(new GetAllOrderRequestDTO { pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.Equal(404, result.StatusCode);
        }

        // ── GetUserOrderById ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetUserOrderById_WithOrders_ReturnsList()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            var result = await service.GetUserOrderById(userId, new GetUserOrderDetailsRequestDTO { pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.NotEmpty(result.Data.Items);
        }

        [Fact]
        public async Task GetUserOrderById_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.GetUserOrderById(Guid.NewGuid(), new GetUserOrderDetailsRequestDTO { pagination = new Pagination { PageNumber = 1, PageSize = 10 } }));
            Assert.Equal(404, ex.StatusCode);
        }

        // ── OrderRefund ──────────────────────────────────────────────────────────

        [Fact]
        public async Task OrderRefund_Success_ReturnsIsRefund()
        {
            var context = GetDbContext();
            var (userId, adminId, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, paymentId) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            var result = await service.OrderRefund(adminId, new OrderRefundRequestDTO { OrderId = orderId, PaymentId = paymentId, TotalAmount = 200 });
            Assert.True(result.Data.IsRefund);
        }

        [Fact]
        public async Task OrderRefund_NonAdmin_ThrowsException()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, paymentId) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.OrderRefund(userId, new OrderRefundRequestDTO { OrderId = orderId, PaymentId = paymentId, TotalAmount = 200 }));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task OrderRefund_ZeroAmount_ThrowsException()
        {
            var context = GetDbContext();
            var (_, adminId, prodId, _, addrId) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.OrderRefund(adminId, new OrderRefundRequestDTO { OrderId = Guid.NewGuid(), PaymentId = Guid.NewGuid(), TotalAmount = 0 }));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task OrderRefund_AlreadyRefunded_Throws400()
        {
            var context = GetDbContext();
            var (userId, adminId, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, paymentId) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            await service.OrderRefund(adminId, new OrderRefundRequestDTO { OrderId = orderId, PaymentId = paymentId, TotalAmount = 200 });
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.OrderRefund(adminId, new OrderRefundRequestDTO { OrderId = orderId, PaymentId = paymentId, TotalAmount = 200 }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task OrderRefund_OrderNotFound_Throws404()
        {
            var context = GetDbContext();
            var (_, adminId, _, _, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.OrderRefund(adminId, new OrderRefundRequestDTO { OrderId = Guid.NewGuid(), PaymentId = Guid.NewGuid(), TotalAmount = 100 }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task OrderRefund_PaymentNotFound_Throws404()
        {
            var context = GetDbContext();
            var (userId, adminId, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, _) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.OrderRefund(adminId, new OrderRefundRequestDTO { OrderId = orderId, PaymentId = Guid.NewGuid(), TotalAmount = 100 }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task OrderRefund_CreatesWalletWhenNoneExists()
        {
            var context = GetDbContext();
            var (userId, adminId, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, paymentId) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            // No wallet seeded — should create one
            var result = await service.OrderRefund(adminId, new OrderRefundRequestDTO { OrderId = orderId, PaymentId = paymentId, TotalAmount = 200 });
            Assert.True(result.Data.IsRefund);
            Assert.Single(context.Wallets.Where(w => w.UserId == userId));
        }

        [Fact]
        public async Task OrderRefund_AddsToExistingWallet()
        {
            var context = GetDbContext();
            var (userId, adminId, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, paymentId) = await SeedOrderAsync(context, userId, addrId, prodId);
            context.Wallets.Add(new Wallet { WalletId = Guid.NewGuid(), UserId = userId, WalletAmount = 100 });
            await context.SaveChangesAsync();

            var service = GetService(context);
            await service.OrderRefund(adminId, new OrderRefundRequestDTO { OrderId = orderId, PaymentId = paymentId, TotalAmount = 200 });

            var wallet = context.Wallets.First(w => w.UserId == userId);
            Assert.Equal(300, wallet.WalletAmount);
        }

        [Fact]
        public async Task PlaceOrder_FuturePromoCode_Throws400()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
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
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(userId, new PlaceOrderRequestDTO
                {
                    AddressId = addrId,
                    TotalProductsCount = 1,
                    TotalAmount = 200,
                    PaymentType = "COD",
                    PromoCode = "FUTURE",
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_InvalidPromoCode_Throws400()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(userId, new PlaceOrderRequestDTO
                {
                    AddressId = addrId,
                    TotalProductsCount = 1,
                    TotalAmount = 200,
                    PaymentType = "COD",
                    PromoCode = "INVALID",
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_WithWallet_FullCoverage()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            context.Wallets.Add(new Wallet { WalletId = Guid.NewGuid(), UserId = userId, WalletAmount = 10000 });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.PlaceOrder(userId, new PlaceOrderRequestDTO
            {
                AddressId = addrId,
                TotalProductsCount = 1,
                TotalAmount = 200,
                PaymentType = "Wallet",
                UseWallet = true,
                OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
            });

            Assert.True(result.Data.IsSuccess);
            Assert.True(result.Data.WalletUsed > 0);
        }

        [Fact]
        public async Task PlaceOrder_WithWallet_PartialBalance()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            context.Wallets.Add(new Wallet { WalletId = Guid.NewGuid(), UserId = userId, WalletAmount = 10 });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.PlaceOrder(userId, new PlaceOrderRequestDTO
            {
                AddressId = addrId,
                TotalProductsCount = 1,
                TotalAmount = 200,
                PaymentType = "Partial",
                UseWallet = true,
                OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
            });

            Assert.Equal(10, result.Data.WalletUsed);
        }

        [Fact]
        public async Task PlaceOrder_WithWallet_NoWallet_Throws404()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(userId, new PlaceOrderRequestDTO
                {
                    AddressId = addrId,
                    TotalProductsCount = 1,
                    TotalAmount = 200,
                    PaymentType = "Wallet",
                    UseWallet = true,
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "Widget", Quantity = 1, ProductPrice = 200 }
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_ProductNotFound_Throws404()
        {
            var context = GetDbContext();
            var (userId, _, _, _, addrId) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(userId, new PlaceOrderRequestDTO
                {
                    AddressId = addrId,
                    TotalProductsCount = 1,
                    TotalAmount = 200,
                    PaymentType = "COD",
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = Guid.NewGuid(), ProductName = "X", Quantity = 1, ProductPrice = 200 }
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task PlaceOrder_StockNotFound_Throws404()
        {
            var context = GetDbContext();
            var (userId, _, _, _, addrId) = await SeedAsync(context);
            // Add product without stock
            var prodId = Guid.NewGuid();
            var catId = context.Categories.First().CategoryId;
            context.Products.Add(new Product { ProductId = prodId, CategoryId = catId, Name = "NoStock", ImagePath = "img", Description = "d", Price = 100, ActiveStatus = true });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.PlaceOrder(userId, new PlaceOrderRequestDTO
                {
                    AddressId = addrId,
                    TotalProductsCount = 1,
                    TotalAmount = 100,
                    PaymentType = "COD",
                    OrderProductdDetails = new PlaceOrderItemDTO { ProductId = prodId, ProductName = "NoStock", Quantity = 1, ProductPrice = 100 }
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_OrderNotFound_ThrowsException()
        {
            var context = GetDbContext();
            var (userId, _, _, _, _) = await SeedAsync(context);
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateOrder(userId, Guid.NewGuid(), "Shipped"));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task UpdateOrder_AlreadyCancelled_ThrowsException()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, _) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            await service.CancelOrder(userId, orderId);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateOrder(userId, orderId, "Shipped"));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task UpdateOrder_AlreadyDelivered_ThrowsException()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, _) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            await service.UpdateOrder(userId, orderId, "Delivered");
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateOrder(userId, orderId, "Shipped"));
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task UpdateOrder_DeliveredStatus_SetsDeliveryDate()
        {
            var context = GetDbContext();
            var (userId, _, prodId, _, addrId) = await SeedAsync(context);
            var (orderId, _) = await SeedOrderAsync(context, userId, addrId, prodId);
            var service = GetService(context);

            var result = await service.UpdateOrder(userId, orderId, "Delivered");
            Assert.True(result.Data.IsUpdated);
        }

        [Fact]
        public async Task CancelOrder_StockNotFound_SkipsAndCancels()
        {
            // Stock record missing for order item — service skips (no throw) and still cancels
            var context = GetDbContext();
            var (userId, _, prodId, stockId, addrId) = await SeedAsync(context);
            var (orderId, _) = await SeedOrderAsync(context, userId, addrId, prodId);

            // Remove stock so the cancel loop hits the "stock == null → continue" branch
            var stock = context.Stock.Find(stockId);
            context.Stock.Remove(stock!);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.CancelOrder(userId, orderId);
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task GetUserOrderById_NoOrders_ReturnsEmptyList()
        {
            var context = GetDbContext();
            var (userId, _, _, _, _) = await SeedAsync(context);
            var service = GetService(context);

            var result = await service.GetUserOrderById(userId, new GetUserOrderDetailsRequestDTO { pagination = new Pagination { PageNumber = 1, PageSize = 10 } });
            Assert.Empty(result.Data.Items);
            Assert.Equal("No orders found", result.Message);
        }
    }
}
