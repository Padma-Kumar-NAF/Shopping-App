using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Category;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface ICategoryController
    {
        public Task<IActionResult> AddCategory(AddCategoryRequestDTO request);
        public Task<IActionResult> DeleteCategory(DeleteCategoryRequestDTO request);
        public Task<IActionResult> EditCategory(EditCategoryRequestDTO request);
        public Task<IActionResult> GetAllCategories(GetAllCategoryRequestDTO request);
        public Task<IActionResult> GetProductsByCategory(GetProductsByCategoryRequestDTO request);
    }
}