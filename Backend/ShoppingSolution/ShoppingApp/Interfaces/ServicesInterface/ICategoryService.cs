using ShoppingApp.Models;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICategoryService
    {
        public Task<Category> AddCategory(string categoryName);

        public Task<IEnumerable<Category>> GetAllCategories(int Limit,int PageNumber);
    }
}
