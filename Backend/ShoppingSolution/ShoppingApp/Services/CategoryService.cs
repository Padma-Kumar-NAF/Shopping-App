using Microsoft.EntityFrameworkCore;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;

namespace ShoppingApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Guid,Category> _repository;
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

            return await _repository.AddAsync(category);
        }

        public async Task<IEnumerable<Category>> GetAllCategories(int Limit, int PageNumber)
        {
            if (Limit <= 0)
                throw new ArgumentException("Limit must be greater than 0.");

            if (PageNumber <= 0)
                throw new ArgumentException("PageNumber must be greater than 0.");

            return await _repository
                .GetQueryable()
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .Skip((PageNumber - 1) * Limit)
                .Take(Limit)
                .ToListAsync();
        }

        //public Task<Category> DeleteCategory(Guid CategoryId)
        //{

        //}
    }
}
