using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
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
        private readonly IRepository<Guid, Product> _productRepository;

        public CategoryService(IRepository<Guid, Category> categoryRepository, IRepository<Guid, Product> productRepository)
        {
            _repository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<ApiResponse<AddCategoryResponseDTO>> AddCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new AppException("Category name is required", 400);
            }

            string CategoryName = categoryName.Trim();
            var existCategory = await _repository.FirstOrDefaultAsync(c => c.CategoryName == CategoryName);

            if (existCategory != null)
            {
                throw new AppException("Category already exists", 409);
            }

            var category = new Category
            {
                CategoryName = CategoryName
            };

            try
            {
                await _repository.AddAsync(category);
            }
            catch (DbUpdateException)
            {
                throw new AppException("Error while updating category", 500);
            }
            

            return new ApiResponse<AddCategoryResponseDTO>
            {
                StatusCode = 200,
                Message = "Category name added",
                Data = new AddCategoryResponseDTO()
                {
                    CategoryId = category.CategoryId,
                },
                Action = "AddCategory"
            };
        }

        public async Task<ApiResponse<DeleteCategoryResponseDTO>> DeleteCategory(DeleteCategoryRequestDTO request)
        {
            if (request.CategoryId == Guid.Empty)
                throw new AppException("Invalid Category Id", 400);

            var category = await _repository.GetAsync(request.CategoryId);

            if (category == null)
                throw new AppException("Category not found", 404);

            var isUsed = await _productRepository
                .GetQueryable()
                .AnyAsync(p => p.CategoryId == request.CategoryId);

            if (isUsed)
                throw new AppException("Cannot delete category. It is associated with products.", 400);

            try
            {
                await _repository.DeleteAsync(request.CategoryId);
            }
            catch (DbUpdateException)
            {
                throw new AppException("Error while deleting category", 500);
            }

            return new ApiResponse<DeleteCategoryResponseDTO>()
            {
                Data = new DeleteCategoryResponseDTO
                {
                    IsSuccess = true,
                },
                StatusCode = 200,
                Message = "Category deleted successfully",
                Action = "DeleteCategory"
            };
        }

        public async Task<ApiResponse<EditCategoryResponseDTO>> EditCategory(EditCategoryRequestDTO request)
        {
            if (request.CategoryId == Guid.Empty)
                throw new AppException("Invalid Category Id", 400);

            if (string.IsNullOrWhiteSpace(request.CategoryName))
                throw new AppException("Category name is required", 400);

            string categoryName = request.CategoryName.Trim();

            var category = await _repository.GetAsync(request.CategoryId);

            if (category == null)
                throw new AppException("Category not found", 404);

            var existingCategory = await _repository
                .FirstOrDefaultAsync(c => c.CategoryName == categoryName && c.CategoryId != request.CategoryId);

            if (existingCategory != null)
                throw new AppException("Category name already exists", 409);

            category.CategoryName = categoryName;

            try
            {
                await _repository.UpdateAsync(request.CategoryId, category);
            }
            catch (DbUpdateException)
            {
                throw new AppException("Error while updating category", 500);
            }

            return new ApiResponse<EditCategoryResponseDTO>()
            {
                StatusCode = 200,
                Data = new EditCategoryResponseDTO
                {
                    IsSuccess = true
                },
                Action = "EditCategory",
                Message = "Category updated successfully",
            };
        }

        public async Task<ApiResponse<GetAllCategoryResponseDTO>> GetAllCategories(int limit, int pageNumber)
        {
            if (limit <= 0)
                throw new AppException("Page size must be greater than 0", 400);

            if (pageNumber <= 0)
                throw new AppException("Page number must be greater than 0", 400);

            var categories = await _repository
                .GetQueryable()
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .Skip((pageNumber - 1) * limit)
                .Take(limit)
                .Select(c => new CategoryDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            if (!categories.Any())
                throw new AppException("No categories found", 404);

            return new ApiResponse<GetAllCategoryResponseDTO>()
            {
                StatusCode = 200,
                Message = "Categories fetched successfully",
                Data = new GetAllCategoryResponseDTO
                {
                    CategoryList = categories
                },
                Action = "GetAllCategories"
            };
        }

        public async Task<ApiResponse<GetProductsByCategoryResponseDTO>> GetProductsByCategory(GetProductsByCategoryRequestDTO request)
        {
            if (request.CategoryId == Guid.Empty)
                throw new AppException("Invalid Category Id", 400);

            var category = await _repository.GetAsync(request.CategoryId);

            if (category == null)
                throw new AppException("Category not found", 404);

            var products = await _productRepository
                .GetQueryable()
                .Where(p => p.CategoryId == request.CategoryId)
                .Select(p => new ProductsDTO
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    StockId = p.Stock!.StockId,
                    Name = p.Name,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    CategoryName = p.Category!.CategoryName,
                    Price = p.Price,
                    Quantity = p.Stock.Quantity,

                    Review = p.Reviews != null
                        ? p.Reviews.Select(r => new CategoryProductReviewDTO
                        {
                            Summary = r.Summary,
                            ReviewPoints = r.ReviewPoints
                        }).ToList()
                        : new List<CategoryProductReviewDTO>()
                })
                .ToListAsync();

            var response = new GetProductsByCategoryResponseDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Products = products ?? new List<ProductsDTO>()
            };

            return new ApiResponse<GetProductsByCategoryResponseDTO>()
            {
                Data = response,
                StatusCode = 200,
                Message = products.Any()
                    ? "Products fetched successfully"
                    : "No products found for this category",
                Action = "GetProductsByCategory"
            };
        }
    }
}