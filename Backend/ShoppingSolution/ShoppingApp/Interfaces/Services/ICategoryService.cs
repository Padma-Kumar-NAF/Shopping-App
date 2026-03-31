using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Category;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICategoryService
    {
        public Task<ApiResponse<AddCategoryResponseDTO>> AddCategory(string categoryName);
        public Task<ApiResponse<GetAllCategoryResponseDTO>> GetAllCategories(int Limit,int PageNumber);
        public Task<ApiResponse<EditCategoryResponseDTO>> EditCategory(EditCategoryRequestDTO request);
        public Task<ApiResponse<DeleteCategoryResponseDTO>> DeleteCategory(DeleteCategoryRequestDTO request);
        public Task<ApiResponse<GetProductsByCategoryResponseDTO>> GetProductsByCategory(GetProductsByCategoryRequestDTO request);
    }
}