using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.UserMonthlyProductLimit;
using ShoppingApp.Models.Entities;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class UserMonthlyProductLimitServiceTests
    {
        // ── Infrastructure ────────────────────────────────────────────────────

        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private UserMonthlyProductLimitService GetService(ShoppingContext context)
            => new UserMonthlyProductLimitService(
                new Repository<Guid, UserMonthlyProductLimit>(context),
                new Repository<Guid, Product>(context));

        private async Task<Guid> SeedProductAsync(ShoppingContext context, Guid? productId = null)
        {
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                CategoryName = $"Cat-{Guid.NewGuid()}",
                CreatedAt = DateTime.UtcNow
            };
            context.Categories.Add(category);

            var id = productId ?? Guid.NewGuid();
            context.Products.Add(new Product
            {
                ProductId = id,
                CategoryId = category.CategoryId,
                Name = $"Product-{id}",
                Description = "Test product",
                Price = 10m,
                ImagePath = "img.png",
                ActiveStatus = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            return id;
        }

        private AddUserMonthlyProductLimitRequestDTO AddRequest(Guid productId, int limit = 5)
            => new AddUserMonthlyProductLimitRequestDTO { ProductId = productId, MonthlyLimit = limit };

        private GetAllUserMonthlyProductLimitRequestDTO PageRequest(int page = 1, int size = 10)
            => new GetAllUserMonthlyProductLimitRequestDTO
            {
                Pagination = new Pagination { PageNumber = page, PageSize = size }
            };

        // ── AddLimit ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AddLimit_Success_ReturnsId()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            var result = await service.AddLimit(AddRequest(productId));

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Monthly product limit added successfully", result.Message);
            Assert.Equal("AddLimit", result.Action);
            Assert.NotEqual(Guid.Empty, result.Data!.Id);
        }

        [Fact]
        public async Task AddLimit_ProductNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddLimit(AddRequest(Guid.NewGuid())));

            Assert.Equal(404, ex.StatusCode);
            Assert.Equal("Product not found", ex.Message);
        }

        [Fact]
        public async Task AddLimit_DuplicateProduct_Throws409()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            await service.AddLimit(AddRequest(productId));

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddLimit(AddRequest(productId)));

            Assert.Equal(409, ex.StatusCode);
            Assert.Equal("A monthly limit for this product already exists", ex.Message);
        }

        [Fact]
        public async Task AddLimit_RepositoryReturnsNull_Throws500()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);

            var mockRepo = new Mock<IRepository<Guid, UserMonthlyProductLimit>>();
            mockRepo.Setup(r => r.GetQueryable())
                .Returns(context.UserMonthlyProductLimit.AsQueryable());
            mockRepo.Setup(r => r.AddAsync(It.IsAny<UserMonthlyProductLimit>()))
                .ReturnsAsync((UserMonthlyProductLimit?)null);

            var service = new UserMonthlyProductLimitService(
                mockRepo.Object,
                new Repository<Guid, Product>(context));

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddLimit(AddRequest(productId)));

            Assert.Equal(500, ex.StatusCode);
            Assert.Equal("Unable to add monthly product limit at this moment", ex.Message);
        }

        [Fact]
        public async Task AddLimit_StoresCorrectValues()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            var result = await service.AddLimit(AddRequest(productId, limit: 7));

            var stored = await context.UserMonthlyProductLimit.FindAsync(result.Data!.Id);
            Assert.NotNull(stored);
            Assert.Equal(productId, stored!.ProductId);
            Assert.Equal(7, stored.MonthlyLimit);
        }

        // ── EditLimit ─────────────────────────────────────────────────────────

        [Fact]
        public async Task EditLimit_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            var added = await service.AddLimit(AddRequest(productId, limit: 3));

            var result = await service.EditLimit(new EditUserMonthlyProductLimitRequestDTO
            {
                Id = added.Data!.Id,
                MonthlyLimit = 10
            });

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Monthly product limit updated successfully", result.Message);
            Assert.Equal("EditLimit", result.Action);
            Assert.True(result.Data!.IsSuccess);
        }

        [Fact]
        public async Task EditLimit_UpdatesPersisted()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            var added = await service.AddLimit(AddRequest(productId, limit: 3));

            await service.EditLimit(new EditUserMonthlyProductLimitRequestDTO
            {
                Id = added.Data!.Id,
                MonthlyLimit = 15
            });

            var stored = await context.UserMonthlyProductLimit.FindAsync(added.Data.Id);
            Assert.Equal(15, stored!.MonthlyLimit);
        }

        [Fact]
        public async Task EditLimit_NotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditLimit(new EditUserMonthlyProductLimitRequestDTO
                {
                    Id = Guid.NewGuid(),
                    MonthlyLimit = 5
                }));

            Assert.Equal(404, ex.StatusCode);
            Assert.Equal("Monthly product limit not found", ex.Message);
        }

        [Fact]
        public async Task EditLimit_SameValue_ReturnsNoChangesRequired()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            var added = await service.AddLimit(AddRequest(productId, limit: 5));

            var result = await service.EditLimit(new EditUserMonthlyProductLimitRequestDTO
            {
                Id = added.Data!.Id,
                MonthlyLimit = 5   // same as existing
            });

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("No changes required", result.Message);
            Assert.Equal("EditLimit", result.Action);
            Assert.True(result.Data!.IsSuccess);
        }

        // ── DeleteLimit ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteLimit_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            var added = await service.AddLimit(AddRequest(productId));

            var result = await service.DeleteLimit(new DeleteUserMonthlyProductLimitRequestDTO
            {
                Id = added.Data!.Id
            });

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Monthly product limit deleted successfully", result.Message);
            Assert.Equal("DeleteLimit", result.Action);
            Assert.True(result.Data!.IsSuccess);
        }

        [Fact]
        public async Task DeleteLimit_RecordRemovedFromDb()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            var added = await service.AddLimit(AddRequest(productId));
            var id = added.Data!.Id;

            await service.DeleteLimit(new DeleteUserMonthlyProductLimitRequestDTO { Id = id });

            var stored = await context.UserMonthlyProductLimit.FindAsync(id);
            Assert.Null(stored);
        }

        [Fact]
        public async Task DeleteLimit_NotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteLimit(new DeleteUserMonthlyProductLimitRequestDTO
                {
                    Id = Guid.NewGuid()
                }));

            Assert.Equal(404, ex.StatusCode);
            Assert.Equal("Monthly product limit not found", ex.Message);
        }

        [Fact]
        public async Task DeleteLimit_RepositoryReturnsNull_Throws500()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);

            var existingLimit = new UserMonthlyProductLimit
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                MonthlyLimit = 5,
                CreatedAt = DateTime.UtcNow
            };

            var mockRepo = new Mock<IRepository<Guid, UserMonthlyProductLimit>>();
            mockRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserMonthlyProductLimit, bool>>>()))
                .ReturnsAsync(existingLimit);
            mockRepo
                .Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
                .ReturnsAsync((UserMonthlyProductLimit?)null);

            var service = new UserMonthlyProductLimitService(
                mockRepo.Object,
                new Repository<Guid, Product>(context));

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteLimit(new DeleteUserMonthlyProductLimitRequestDTO
                {
                    Id = existingLimit.Id
                }));

            Assert.Equal(500, ex.StatusCode);
            Assert.Equal("Failed to delete monthly product limit", ex.Message);
        }

        // ── GetAllLimits ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllLimits_Empty_ReturnsEmptyResponse()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllLimits(PageRequest());

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("No records found", result.Message);
            Assert.Equal("GetAllLimits", result.Action);
            Assert.Empty(result.Data!.Records);
            Assert.Equal(0, result.Data.TotalCount);
            Assert.Equal(1, result.Data.PageNumber);
            Assert.Equal(10, result.Data.PageSize);
        }

        [Fact]
        public async Task GetAllLimits_WithData_ReturnsRecords()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            await service.AddLimit(AddRequest(productId, limit: 4));

            var result = await service.GetAllLimits(PageRequest());

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Records fetched successfully", result.Message);
            Assert.Equal("GetAllLimits", result.Action);
            Assert.Single(result.Data!.Records);
            Assert.Equal(1, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetAllLimits_RecordFields_AreCorrect()
        {
            var context = GetDbContext();
            var productId = await SeedProductAsync(context);
            var service = GetService(context);

            var added = await service.AddLimit(AddRequest(productId, limit: 8));

            var result = await service.GetAllLimits(PageRequest());
            var record = result.Data!.Records.First();

            Assert.Equal(added.Data!.Id, record.Id);
            Assert.Equal(productId, record.ProductId);
            Assert.Equal(8, record.MonthlyLimit);
            Assert.False(string.IsNullOrEmpty(record.ProductName));
        }

        [Fact]
        public async Task GetAllLimits_Pagination_ReturnsCorrectPage()
        {
            var context = GetDbContext();
            var service = GetService(context);

            // Seed 3 products and limits
            for (int i = 0; i < 3; i++)
            {
                var pid = await SeedProductAsync(context);
                await service.AddLimit(AddRequest(pid, limit: i + 1));
            }

            var page1 = await service.GetAllLimits(PageRequest(page: 1, size: 2));
            Assert.Equal(2, page1.Data!.Records.Count);
            Assert.Equal(3, page1.Data.TotalCount);
            Assert.Equal(1, page1.Data.PageNumber);
            Assert.Equal(2, page1.Data.PageSize);

            var page2 = await service.GetAllLimits(PageRequest(page: 2, size: 2));
            Assert.Single(page2.Data!.Records);
            Assert.Equal(3, page2.Data.TotalCount);
            Assert.Equal(2, page2.Data.PageNumber);
        }

        [Fact]
        public async Task GetAllLimits_Empty_ReturnsCorrectPaginationMeta()
        {
            // Verifies that PageNumber and PageSize from the request are echoed
            // back in the empty-result response (the totalCount == 0 branch).
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetAllLimits(PageRequest(page: 3, size: 25));

            Assert.Equal(0, result.Data!.TotalCount);
            Assert.Equal(3, result.Data.PageNumber);
            Assert.Equal(25, result.Data.PageSize);
            Assert.Empty(result.Data.Records);
        }

        [Fact]
        public async Task GetAllLimits_OrderedByCreatedAt()
        {
            var context = GetDbContext();
            var service = GetService(context);

            // Seed with explicit CreatedAt values to verify ordering
            var pid1 = await SeedProductAsync(context);
            var pid2 = await SeedProductAsync(context);
            var pid3 = await SeedProductAsync(context);

            context.UserMonthlyProductLimit.AddRange(
                new UserMonthlyProductLimit { Id = Guid.NewGuid(), ProductId = pid1, MonthlyLimit = 1, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new UserMonthlyProductLimit { Id = Guid.NewGuid(), ProductId = pid2, MonthlyLimit = 2, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new UserMonthlyProductLimit { Id = Guid.NewGuid(), ProductId = pid3, MonthlyLimit = 3, CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var result = await service.GetAllLimits(PageRequest());
            var limits = result.Data!.Records.Select(r => r.MonthlyLimit).ToList();

            Assert.Equal(new[] { 1, 2, 3 }, limits);
        }
    }
}
