using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
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
        [HttpPost("add-category")]
        [ValidateRequest]
        public async Task<IActionResult> AddCategory(AddCategoryRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                {
                    return BadRequest("User not found");
                }

                var result = await _categoryService.AddCategory(request.CategoryName);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete("delete-category")]
        [ValidateRequest]
        public async Task<IActionResult> DeleteCategory(DeleteCategoryRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                {
                    return BadRequest("User not found");
                }
                var result  = await _categoryService.DeleteCategory(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "Admin,User")]
        [HttpPost("get-all-categories")]
        [ValidateRequest]
        public async Task<IActionResult> GetAllCategories([FromBody] GetAllCategoryRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                {
                    return BadRequest("User not found");
                }
                var CategoriesList = await _categoryService.GetAllCategories(request.Pagination.PageSize,request.Pagination.PageNumber);
                return Ok(CategoriesList);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("edit-category")]
        [ValidateRequest]
        public async Task<IActionResult> EditCategory([FromBody] EditCategoryRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                {
                    return BadRequest("User not found");
                }
                var result = await _categoryService.EditCategory(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("products-by-category")]
        public async Task<IActionResult> GetProductsByCategory([FromBody] GetProductsByCategoryRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty )
                {
                    return BadRequest("User not found");
                }

                var result = await _categoryService.GetProductsByCategory(request);
                return StatusCode(result.StatusCode, result);
            }
            catch
            {
                throw;
            }
        }
    }
}
