using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Category;

namespace ShoppingApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase, ICategoryController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("AddCategory")]
        public async Task<ActionResult<AddCategoryResponseDTO>> AddCategory(AddCategoryRequestDTO request)
        {
            try
            {
                var addedCategory = await _categoryService.AddCategory(request.CategoryName);
                AddCategoryResponseDTO responseDTO = new AddCategoryResponseDTO();
                responseDTO.CategoryName = addedCategory.CategoryName;
                responseDTO.CategoryId = addedCategory.CategoryId;
                return Ok(responseDTO);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("GetAllCategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories([FromBody] GetAllCategoryRequestDTO request)
        {
            try
            {
                var CategoriesList = await _categoryService.GetAllCategories(request.Limit,request.PageNumber);
                if(CategoriesList == null)
                {
                    return BadRequest("No Categories Found");
                }
                return Ok(CategoriesList);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
