using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Category;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface ICategoryController
    {
        public Task<ActionResult<AddCategoryResponseDTO>> AddCategory(AddCategoryRequestDTO request);
        public Task<ActionResult<IEnumerable<Category>>> GetAllCategories(GetAllCategoryRequestDTO request);
    }
}
