using Microsoft.EntityFrameworkCore;
using Moq;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.Product;
using ShoppingApp.Repositories;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class ProductServiceTests
    {
        private ShoppingContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ShoppingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoppingContext(options);
        }

        private (ProductService service, ShoppingContext context) GetService()
        {
            var context = GetDbContext();
            var service = new ProductService(
                new Repository<Guid, Stock>(context),
                new Repository<Guid, Product>(context),
                new Repository<Guid, Category>(context),
                new Repository<Guid, User>(context),
                new UnitOfWork(context)
            );
            return (service, context);
        }

        private async Task<(Guid userId, Guid categoryId, Guid productId, Guid stockId)> SeedProductAsync(ShoppingContext context)
        {
            var userId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            var prodId = Guid.NewGuid();
            var stockId = Guid.NewGuid();

            context.Users.Add(new User { UserId = userId, Name = "Admin", Email = "a@a.com", Password = "x", SaltValue = "s", Role = "admin", Active = true });
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Cat" });
            context.Products.Add(new Product { ProductId = prodId, CategoryId = catId, Name = "Widget", ImagePath = "img", Description = "desc", Price = 100, ActiveStatus = true });
            context.Stock.Add(new Stock { StockId = stockId, ProductId = prodId, Quantity = 10 });
            await context.SaveChangesAsync();

            return (userId, catId, prodId, stockId);
        }

        // ── AddProduct ───────────────────────────────────────────────────────────

        [Fact]
        public async Task AddProduct_Success_ReturnsProductId()
        {
            var (service, context) = GetService();
            var (userId, catId, _, _) = await SeedProductAsync(context);

            var result = await service.AddProduct(userId, new AddNewProductRequestDTO
            {
                CategoryId = catId,
                Name = "NewProd",
                ImagePath = "img",
                Description = "desc",
                Price = 50,
                Quantity = 5
            });

            Assert.NotEqual(Guid.Empty, result.Data.ProductId);
        }

        [Fact]
        public async Task AddProduct_UserNotFound_Throws404()
        {
            var (service, _) = GetService();
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddProduct(Guid.NewGuid(), new AddNewProductRequestDTO
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "X",
                    ImagePath = "img",
                    Description = "desc",
                    Price = 10,
                    Quantity = 1
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AddProduct_CategoryNotFound_Throws404()
        {
            var (service, context) = GetService();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.AddProduct(userId, new AddNewProductRequestDTO
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "X",
                    ImagePath = "img",
                    Description = "desc",
                    Price = 10,
                    Quantity = 1
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        // ── GetProducts ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetProducts_WithData_ReturnsList()
        {
            var (service, context) = GetService();
            await SeedProductAsync(context);

            var result = await service.GetProducts(new GetAllProductsRequestDTO
            {
                pagination = new Pagination { PageNumber = 1, PageSize = 10 }
            });

            Assert.NotEmpty(result.Data.ProductList);
        }

        [Fact]
        public async Task GetProducts_Empty_ReturnsEmptyMessage()
        {
            var (service, _) = GetService();
            var result = await service.GetProducts(new GetAllProductsRequestDTO
            {
                pagination = new Pagination { PageNumber = 1, PageSize = 10 }
            });
            Assert.Equal("No products available for this moment", result.Message);
        }

        [Fact]
        public async Task GetProducts_GenericException_BubblesUp()
        {
            var mockStock = new Mock<IRepository<Guid, Stock>>();
            var mockProd = new Mock<IRepository<Guid, Product>>();
            var mockCat = new Mock<IRepository<Guid, Category>>();
            var mockUser = new Mock<IRepository<Guid, User>>();
            var mockUow = new Mock<IUnitOfWork>();

            mockProd.Setup(x => x.GetQueryable()).Throws(new Exception("Unexpected"));

            var service = new ProductService(mockStock.Object, mockProd.Object, mockCat.Object, mockUser.Object, mockUow.Object);
            await Assert.ThrowsAsync<Exception>(() =>
                service.GetProducts(new GetAllProductsRequestDTO { pagination = new Pagination { PageNumber = 1, PageSize = 10 } }));
        }

        // ── GetProductsWithFilter ────────────────────────────────────────────────

        [Fact]
        public async Task GetProductsWithFilter_LowPriceGreaterThanHigh_Throws400()
        {
            var (service, _) = GetService();
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.GetProductsWithFilter(new GetAllProductsWithFilterRequestDTO
                {
                    LowPrice = 500,
                    HighPrice = 100,
                    pagination = new Pagination { PageNumber = 1, PageSize = 10 }
                }));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task GetProductsWithFilter_NoMatch_ReturnsEmptyMessage()
        {
            var (service, _) = GetService();
            var result = await service.GetProductsWithFilter(new GetAllProductsWithFilterRequestDTO
            {
                LowPrice = 9999,
                HighPrice = 99999,
                pagination = new Pagination { PageNumber = 1, PageSize = 10 }
            });
            Assert.Equal("No products found for the selected price range", result.Message);
        }

        [Fact]
        public async Task GetProductsWithFilter_WithCategoryFilter_ReturnsFiltered()
        {
            var (service, context) = GetService();
            var (_, catId, _, _) = await SeedProductAsync(context);

            var result = await service.GetProductsWithFilter(new GetAllProductsWithFilterRequestDTO
            {
                LowPrice = 0,
                HighPrice = 9999,
                CategoryId = catId,
                pagination = new Pagination { PageNumber = 1, PageSize = 10 }
            });

            Assert.NotEmpty(result.Data.ProductList);
        }

        // ── GetSuggestions ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetSuggestions_EmptyQuery_ReturnsEmpty()
        {
            var (service, _) = GetService();
            var result = await service.GetSuggestions("   ");
            Assert.Empty(result.Data);
            Assert.Equal("Empty query", result.Message);
        }

        [Fact]
        public async Task GetSuggestions_WithMatch_ReturnsSuggestions()
        {
            var (service, context) = GetService();
            await SeedProductAsync(context);

            var result = await service.GetSuggestions("Wid");
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public async Task GetSuggestions_NoMatch_ReturnsEmptyList()
        {
            var (service, context) = GetService();
            await SeedProductAsync(context);

            var result = await service.GetSuggestions("ZZZZZ");
            Assert.Empty(result.Data);
        }

        // ── SearchProductById ────────────────────────────────────────────────────

        [Fact]
        public async Task SearchProductById_Found_ReturnsProduct()
        {
            var (service, context) = GetService();
            var (_, _, prodId, _) = await SeedProductAsync(context);

            var result = await service.SearchProductById(new SearchProductByIdRequestDTO { ProductId = prodId });
            Assert.Equal(prodId, result.Data.ProductId);
        }

        [Fact]
        public async Task SearchProductById_NotFound_Throws404()
        {
            var (service, _) = GetService();
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.SearchProductById(new SearchProductByIdRequestDTO { ProductId = Guid.NewGuid() }));
            Assert.Equal(404, ex.StatusCode);
        }

        // ── SearchProductByName ──────────────────────────────────────────────────

        [Fact]
        public async Task SearchProductByName_Found_ReturnsList()
        {
            var (service, context) = GetService();
            await SeedProductAsync(context);

            var result = await service.SearchProductByName(new SearchProductByNameRequestDTO { ProductName = "Widget" });
            Assert.NotEmpty(result.Data.ProductsList);
        }

        [Fact]
        public async Task SearchProductByName_NoMatch_ReturnsEmptyMessage()
        {
            var (service, context) = GetService();
            await SeedProductAsync(context);

            var result = await service.SearchProductByName(new SearchProductByNameRequestDTO { ProductName = "ZZZZZ" });
            Assert.Equal("No matching products found", result.Message);
        }

        // ── UpdateProduct ────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateProduct_Success_ReturnsIsUpdate()
        {
            var (service, context) = GetService();
            var (userId, catId, prodId, _) = await SeedProductAsync(context);

            var result = await service.UpdateProduct(userId, new UpdateProductRequestDTO
            {
                ProductId = prodId,
                CategoryId = catId,
                Name = "Updated",
                ImagePath = "img2",
                Description = "new desc",
                Price = 200,
                Quantity = 20
            });

            Assert.True(result.Data.IsUpdate);
        }

        [Fact]
        public async Task UpdateProduct_UserNotFound_Throws404()
        {
            var (service, _) = GetService();
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateProduct(Guid.NewGuid(), new UpdateProductRequestDTO
                {
                    ProductId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Name = "X",
                    ImagePath = "img",
                    Description = "d",
                    Price = 10,
                    Quantity = 1
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_ProductNotFound_Throws404()
        {
            var (service, context) = GetService();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateProduct(userId, new UpdateProductRequestDTO
                {
                    ProductId = Guid.NewGuid(),
                    CategoryId = Guid.NewGuid(),
                    Name = "X",
                    ImagePath = "img",
                    Description = "d",
                    Price = 10,
                    Quantity = 1
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_CategoryNotFound_Throws404()
        {
            var (service, context) = GetService();
            var (userId, _, prodId, _) = await SeedProductAsync(context);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateProduct(userId, new UpdateProductRequestDTO
                {
                    ProductId = prodId,
                    CategoryId = Guid.NewGuid(), // non-existent category
                    Name = "X",
                    ImagePath = "img",
                    Description = "d",
                    Price = 10,
                    Quantity = 1
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        // ── DeleteProduct ────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteProduct_Success_SetsActiveStatusFalse()
        {
            var (service, context) = GetService();
            var (userId, _, prodId, _) = await SeedProductAsync(context);

            var result = await service.DeleteProduct(userId, new DeleteProductRequestDTO { ProductId = prodId });
            Assert.True(result.Data.IsDeleted);

            var product = await context.Products.FindAsync(prodId);
            Assert.False(product!.ActiveStatus);
        }

        [Fact]
        public async Task DeleteProduct_UserNotFound_Throws404()
        {
            var (service, _) = GetService();
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteProduct(Guid.NewGuid(), new DeleteProductRequestDTO { ProductId = Guid.NewGuid() }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_ProductNotFound_Throws404()
        {
            var (service, context) = GetService();
            var userId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.DeleteProduct(userId, new DeleteProductRequestDTO { ProductId = Guid.NewGuid() }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_StockNotFound_Throws404()
        {
            var (service, context) = GetService();
            var userId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            var prodId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Email = "u@u.com", Password = "x", SaltValue = "s", Role = "user", Active = true });
            context.Categories.Add(new Category { CategoryId = catId, CategoryName = "Cat" });
            context.Products.Add(new Product { ProductId = prodId, CategoryId = catId, Name = "P", ImagePath = "img", Description = "d", Price = 10, ActiveStatus = true });
            // No stock
            await context.SaveChangesAsync();

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.UpdateProduct(userId, new UpdateProductRequestDTO
                {
                    ProductId = prodId,
                    CategoryId = catId,
                    Name = "Updated",
                    ImagePath = "img",
                    Description = "d",
                    Price = 20,
                    Quantity = 5
                }));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetProductsWithFilter_NoCategoryFilter_ReturnsAll()
        {
            var (service, context) = GetService();
            await SeedProductAsync(context);

            var result = await service.GetProductsWithFilter(new GetAllProductsWithFilterRequestDTO
            {
                LowPrice = 0,
                HighPrice = 9999,
                CategoryId = null,
                pagination = new Pagination { PageNumber = 1, PageSize = 10 }
            });

            Assert.NotEmpty(result.Data.ProductList);
        }
    }
}
