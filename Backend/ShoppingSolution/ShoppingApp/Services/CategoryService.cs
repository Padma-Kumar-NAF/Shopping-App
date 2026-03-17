using Microsoft.EntityFrameworkCore;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Category;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Guid, Category> _repository;

        public CategoryService(IRepository<Guid, Category> categoryRepository)
        {
            _repository = categoryRepository;
        }

        public async Task<Category> AddCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentException("Category name is required.");

            var category = new Category
            {
                CategoryName = categoryName.Trim()
            };

            await _repository.AddAsync(category);

            return category;
        }

        public async Task<ICollection<GetAllCategoryResponseDTO>> GetAllCategories(int limit, int pageNumber)
        {
            if (limit <= 0)
                throw new ArgumentException("Limit must be greater than 0.");

            if (pageNumber <= 0)
                throw new ArgumentException("PageNumber must be greater than 0.");

            var categories = await _repository
                .GetQueryable()
                .AsNoTracking()
                .Include(c => c.Products!)
                    .ThenInclude(p => p.Stock)
                .Include(c => c.Products!)
                    .ThenInclude(p => p.Reviews)
                .OrderBy(c => c.CategoryName)
                .Skip((pageNumber - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return categories.Select(c => new GetAllCategoryResponseDTO
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,

                Products = (c.Products ?? new List<Product>())
                    .Select(p => new GetAllProductsResponseDTO
                    {
                        ProductId = p.ProductId,
                        CategoryId = p.CategoryId,

                        StockId = p.Stock?.StockId ?? Guid.Empty,
                        Quantity = p.Stock?.Quantity ?? 0,

                        Name = p.Name,
                        ImagePath = p.ImagePath,
                        Description = p.Description,
                        CategoryName = c.CategoryName,
                        Price = p.Price,

                        Review = (p.Reviews ?? new List<Review>())
                            .Select(r => new ReviewDTO
                            {
                                Summary = r.Summary,
                                ReviewPoints = r.ReviewPoints
                            }).ToList()
                    }).ToList()

            }).ToList();
        }
    }
}