using Microsoft.EntityFrameworkCore;
using Moq;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Category;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using System.Linq.Expressions;
using Xunit;

namespace Testing.Services
{
    public class CategoryServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private CategoryService GetService(ShoppingContext context)
        {
            return new CategoryService(
                new Repository<Guid, Category>(context),
                new Repository<Guid, Product>(context)
            );
        }

        // ── AddCategory ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AddCategory_Success_ReturnsNewCategoryId()
        {
            var service = GetService(GetDbContext());
            var result = await service.AddCategory("Electronics");
            Assert.NotEqual(Guid.Empty, result.Data.CategoryId);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task AddCategory_EmptyName_Throws400()
        {
            var service = GetService(GetDbContext());
            var ex = await Assert.ThrowsAsync<AppException>(() => service.AddCategory("   "));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task AddCategory_Duplicate_Throws409()
        {
            var context = GetDbContext();
            context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), CategoryName = "Books" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() => service.AddCategory("Books"));
            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task AddCategory_GenericException_Throws500()
        {
            var mockCatRepo = new Mock<IRepository<Guid, Category>>();
            var mockProdRepo = new Mock<IRepository<Guid, Product>>();

            mockCatRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ThrowsAsync(new Exception("Unexpected"));

            var service = new CategoryService(mockCatRepo.Object, mockProdRepo.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() => service.AddCategory("Test"));
            Assert.Equal(500, ex.StatusCode);
        }

        [Fact]
        public async Task AddCategory_DbUpdateException_Throws500()
        {
            var mockCatRepo = new Mock<IRepository<Guid, Category>>();
            var mockProdRepo = new Mock<IRepository<Guid, Product>>();

            mockCatRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ThrowsAsync(new DbUpdateException());

            var service = new CategoryService(mockCatRepo.Object, mockProdRepo.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() => service.AddCategory("Test"));
            Assert.Equal(500, ex.StatusCode);
        }

        // ── GetAllCategories ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllCategories_WithData_ReturnsList()
        {
            var context = GetDbContext();
            context.Categories.Add(new Category { CategoryId = Guid.NewGuid(), CategoryName = "Toys" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetAllCategories(10, 1);
            Assert.NotEmpty(result.Data.CategoryList);
        }

        [Fact]
        public async Task GetAllCategories_Empty_ReturnsEmptyMessage()
        {
            var service = GetService(GetDbContext());
            var result = await service.GetAllCategories(10, 1);
            Assert.Equal("No categories found", result.Message);
        }

        [Fact]
        public async Task GetAllCategories_GenericException_Throws500()
        {
            var mockCatRepo = new Mock<IRepository<Guid, Category>>();
            var mockProdRepo = new Mock<IRepository<Guid, Product>>();

            mockCatRepo.Setup(x => x.GetQueryable()).Throws(new Exception("Unexpected"));

            var service = new CategoryService(mockCatRepo.Object, mockProdRepo.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() => service.GetAllCategories(10, 1));
            Assert.Equal(500, ex.StatusCode);
        }

        // ── EditCategory ─────────────────────────────────────────────────────────

        [Fact]
        public async Task EditCategory_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var catId = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Old" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.EditCategory(new EditCategoryRequestDTO { CategoryId = catId, CategoryName = "New" });
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task EditCategory_EmptyGuid_Throws400()
        {
            var service = GetService(GetDbContext());
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditCategory(new EditCategoryRequestDTO { CategoryId = Guid.Empty, CategoryName = "X" }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task EditCategory_EmptyName_Throws400()
        {
            var service = GetService(GetDbContext());
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditCategory(new EditCategoryRequestDTO { CategoryId = Guid.NewGuid(), CategoryName = "  " }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task EditCategory_NotFound_Throws404()
        {
            var service = GetService(GetDbContext());
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditCategory(new EditCategoryRequestDTO { CategoryId = Guid.NewGuid(), CategoryName = "X" }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task EditCategory_DuplicateName_Throws409()
        {
            var context = GetDbContext();
            var cat1 = new Category { CategoryId = Guid.NewGuid(), CategoryName = "Alpha" };
            var cat2 = new Category { CategoryId = Guid.NewGuid(), CategoryName = "Beta" };
            context.Categories.AddRange(cat1, cat2);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditCategory(new EditCategoryRequestDTO { CategoryId = cat2.CategoryId, CategoryName = "Alpha" }));
            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task EditCategory_DbUpdateException_Throws500()
        {
            var mockCatRepo = new Mock<IRepository<Guid, Category>>();
            var mockProdRepo = new Mock<IRepository<Guid, Product>>();
            var catId = Guid.NewGuid();

            mockCatRepo.Setup(x => x.GetAsync(catId))
                .ReturnsAsync(new Category { CategoryId = catId, CategoryName = "Old" });
            mockCatRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync((Category?)null);
            mockCatRepo.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Category>()))
                .ThrowsAsync(new DbUpdateException());

            var service = new CategoryService(mockCatRepo.Object, mockProdRepo.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.EditCategory(new EditCategoryRequestDTO { CategoryId = catId, CategoryName = "New" }));
            Assert.Equal(500, ex.StatusCode);
        }

        // ── DeleteCategory ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteCategory_Success_ReturnsIsSuccess()
        {
            var context = GetDbContext();
            var catId = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "ToDelete" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.DeleteCategory(new DeleteCategoryRequestDTO { CategoryId = catId });
            Assert.True(result.Data.IsSuccess);
        }

        [Fact]
        public async Task DeleteCategory_EmptyGuid_Throws400()
        {
            var service = GetService(GetDbContext());
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteCategory(new DeleteCategoryRequestDTO { CategoryId = Guid.Empty }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_NotFound_Throws404()
        {
            var service = GetService(GetDbContext());
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteCategory(new DeleteCategoryRequestDTO { CategoryId = Guid.NewGuid() }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_UsedByProduct_Throws400()
        {
            var context = GetDbContext();
            var catId = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "InUse" });
            context.Products.Add(new Product
            {
                ProductId = Guid.NewGuid(),
                CategoryId = catId,
                Name = "P1",
                ImagePath = "img",
                Description = "desc",
                Price = 10,
                ActiveStatus = true
            });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteCategory(new DeleteCategoryRequestDTO { CategoryId = catId }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_GenericException_Throws500()
        {
            var mockCatRepo = new Mock<IRepository<Guid, Category>>();
            var mockProdRepo = new Mock<IRepository<Guid, Product>>();
            var catId = Guid.NewGuid();

            mockCatRepo.Setup(x => x.GetAsync(catId)).ThrowsAsync(new Exception("Unexpected"));

            var service = new CategoryService(mockCatRepo.Object, mockProdRepo.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteCategory(new DeleteCategoryRequestDTO { CategoryId = catId }));
            Assert.Equal(500, ex.StatusCode);
        }

        // ── GetProductsByCategory ────────────────────────────────────────────────

        [Fact]
        public async Task GetProductsByCategory_EmptyGuid_Throws400()
        {
            var service = GetService(GetDbContext());
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.GetProductsByCategory(new GetProductsByCategoryRequestDTO { CategoryId = Guid.Empty }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task GetProductsByCategory_CategoryNotFound_Throws404()
        {
            var service = GetService(GetDbContext());
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.GetProductsByCategory(new GetProductsByCategoryRequestDTO { CategoryId = Guid.NewGuid() }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetProductsByCategory_NoProducts_ReturnsEmptyList()
        {
            var context = GetDbContext();
            var catId = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Empty" });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetProductsByCategory(new GetProductsByCategoryRequestDTO { CategoryId = catId });
            Assert.Empty(result.Data.Products);
        }

        [Fact]
        public async Task GetProductsByCategory_WithProducts_ReturnsList()
        {
            var context = GetDbContext();
            var catId = Guid.NewGuid();
            var prodId = Guid.NewGuid();
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Filled" });
            context.Products.Add(new Product
            {
                ProductId = prodId,
                CategoryId = catId,
                Name = "Widget",
                ImagePath = "img",
                Description = "desc",
                Price = 99,
                ActiveStatus = true
            });
            context.Stock.Add(new Stock { StockId = Guid.NewGuid(), ProductId = prodId, Quantity = 5 });
            await context.SaveChangesAsync();

            var service = GetService(context);
            var result = await service.GetProductsByCategory(new GetProductsByCategoryRequestDTO { CategoryId = catId });
            Assert.Single(result.Data.Products);
        }

        [Fact]
        public async Task GetProductsByCategory_GenericException_Throws500()
        {
            var mockCatRepo = new Mock<IRepository<Guid, Category>>();
            var mockProdRepo = new Mock<IRepository<Guid, Product>>();
            var catId = Guid.NewGuid();

            mockCatRepo.Setup(x => x.GetAsync(catId)).ThrowsAsync(new Exception("Unexpected"));

            var service = new CategoryService(mockCatRepo.Object, mockProdRepo.Object);
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.GetProductsByCategory(new GetProductsByCategoryRequestDTO { CategoryId = catId }));
            Assert.Equal(500, ex.StatusCode);
        }
    }
}
