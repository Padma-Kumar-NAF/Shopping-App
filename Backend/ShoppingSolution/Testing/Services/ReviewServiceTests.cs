using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Review;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class ReviewServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private ReviewService GetService(ShoppingContext context)
        {
            return new ReviewService(
                new Repository<Guid, Review>(context),
                new Repository<Guid, User>(context)
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

        // ── AddReview ────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddReview_Success_ReturnsReviewId()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            var result = await service.AddReview(userId, new AddReviewRequestDTO
            {
                ProductId = prodId,
                Summary = "Great product!",
                ReviewPoints = 5
            });

            Assert.NotEqual(Guid.Empty, result.Data.ReviewId);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task AddReview_UserNotFound_Throws404()
        {
            var context = GetDbContext();
            var service = GetService(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddReview(Guid.NewGuid(), new AddReviewRequestDTO
                {
                    ProductId = Guid.NewGuid(),
                    Summary = "Nice",
                    ReviewPoints = 4
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AddReview_DuplicateReview_Throws409()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            await service.AddReview(userId, new AddReviewRequestDTO { ProductId = prodId, Summary = "Good", ReviewPoints = 4 });

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddReview(userId, new AddReviewRequestDTO { ProductId = prodId, Summary = "Again", ReviewPoints = 3 }));
            Assert.Equal(409, ex.StatusCode);
        }

        // ── DeleteReview ─────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteReview_Success_ReturnsIsDeleted()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            var added = await service.AddReview(userId, new AddReviewRequestDTO { ProductId = prodId, Summary = "Good", ReviewPoints = 4 });
            var reviewId = added.Data.ReviewId;

            var result = await service.DeleteReview(userId, reviewId);
            Assert.True(result.Data.IsDeleted);
        }

        [Fact]
        public async Task DeleteReview_ReviewNotFound_ThrowsException()
        {
            var context = GetDbContext();
            var (userId, _) = await SeedAsync(context);
            var service = GetService(context);

            // Service throws plain Exception (not AppException) when review not found
            await Assert.ThrowsAsync<Exception>(() =>
                service.DeleteReview(userId, Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteReview_WrongUser_ThrowsUnauthorized()
        {
            var context = GetDbContext();
            var (userId, prodId) = await SeedAsync(context);
            var service = GetService(context);

            var added = await service.AddReview(userId, new AddReviewRequestDTO { ProductId = prodId, Summary = "Good", ReviewPoints = 4 });
            var reviewId = added.Data.ReviewId;

            // Service throws UnauthorizedAccessException (not AppException) for wrong user
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.DeleteReview(Guid.NewGuid(), reviewId));
        }
    }
}
