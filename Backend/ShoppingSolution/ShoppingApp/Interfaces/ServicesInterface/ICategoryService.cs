using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Category;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICategoryService
    {
        public Task<Category> AddCategory(string categoryName);

        public Task<ICollection<GetAllCategoryResponseDTO>> GetAllCategories(int Limit,int PageNumber);
    }
}
