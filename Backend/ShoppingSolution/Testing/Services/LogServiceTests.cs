using Microsoft.EntityFrameworkCore;
using Moq;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.Logs;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class LogServiceTests
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

        private LogService GetService(ShoppingContext context)
            => new LogService(new Repository<Guid, Log>(context));

        private GetLogsRequestDTO PageRequest(int page = 1, int size = 10)
            => new GetLogsRequestDTO { Pagination = new Pagination { PageNumber = page, PageSize = size } };

        private Log MakeLog(string message = "Test error", int statusCode = 500) => new Log
        {
            Id = Guid.NewGuid(),
            Message = message,
            InnerException = "inner",
            UserName = "admin",
            Role = "admin",
            Controller = "TestController",
            StatusCode = statusCode,
            CreatedAt = DateTime.UtcNow,
            RowVersion = new byte[8]
        };

        // ── GetLogs ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetLogs_WithData_ReturnsLogs()
        {
            var context = GetDbContext();
            context.Logs.Add(MakeLog());
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetLogs(PageRequest());

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Logs fetched successfully", result.Message);
            Assert.Single(result.Data.Items);
            Assert.Equal(1, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetLogs_Empty_ReturnsNoLogsMessage()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var result = await service.GetLogs(PageRequest());

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("No logs found", result.Message);
            Assert.Empty(result.Data.Items);
            Assert.Equal(0, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetLogs_Pagination_ReturnsCorrectPage()
        {
            var context = GetDbContext();
            for (int i = 0; i < 5; i++)
                context.Logs.Add(MakeLog($"Error {i}"));
            await context.SaveChangesAsync();

            var service = GetService(context);

            var page1 = await service.GetLogs(PageRequest(1, 3));
            Assert.Equal(3, page1.Data.Items.Count);
            Assert.Equal(5, page1.Data.TotalCount);

            var page2 = await service.GetLogs(PageRequest(2, 3));
            Assert.Equal(2, page2.Data.Items.Count);
        }

        [Fact]
        public async Task GetLogs_ReturnsCorrectFields()
        {
            var context = GetDbContext();
            context.Logs.Add(new Log
            {
                Id = Guid.NewGuid(),
                Message = "Field test",
                InnerException = "inner ex",
                UserName = "testuser",
                Role = "user",
                Controller = "HomeController",
                StatusCode = 404,
                CreatedAt = DateTime.UtcNow,
                RowVersion = new byte[8]
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetLogs(PageRequest());
            var item = result.Data.Items.First();

            Assert.Equal("Field test", item.Message);
            Assert.Equal("inner ex", item.InnerException);
            Assert.Equal("testuser", item.UserName);
            Assert.Equal("user", item.Role);
            Assert.Equal("HomeController", item.Controller);
            Assert.Equal(404, item.StatusCode);
        }

        [Fact]
        public async Task GetLogs_RepositoryThrows_WrapsInAppException500()
        {
            var mockRepo = new Mock<IRepository<Guid, Log>>();
            mockRepo.Setup(r => r.GetQueryable()).Throws(new Exception("DB failure"));

            var service = new LogService(mockRepo.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() => service.GetLogs(PageRequest()));

            Assert.Equal(500, ex.StatusCode);
        }
    }
}
