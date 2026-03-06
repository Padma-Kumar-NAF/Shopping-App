using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Category;
using System.Security.Claims;

namespace ShoppingApp.Controllers
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CategoryController : BaseController, ICategoryController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        //[Authorize(Roles = "Admin")]
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

        //[Authorize(Roles = "Admin,User")]
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
