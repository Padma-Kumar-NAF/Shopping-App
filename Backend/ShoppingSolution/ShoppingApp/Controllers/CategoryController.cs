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
    //[Authorize(Roles = "admin")]
    [Route("category")]
    [ApiController]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        //[Authorize(Roles = "admin")]
        [HttpPost("add-category")]
        [ValidateRequest]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _categoryService.AddCategory(request.CategoryName);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin")]
        [HttpDelete("delete-category")]
        [ValidateRequest]
        public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result  = await _categoryService.DeleteCategory(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin,user")]
        [HttpPost("get-all-categories")]
        [ValidateRequest]
        public async Task<IActionResult> GetAllCategories([FromBody] GetAllCategoryRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var CategoriesList = await _categoryService.GetAllCategories(request.Pagination.PageSize,request.Pagination.PageNumber);
                return Ok(CategoriesList);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin")]
        [HttpPost("edit-category")]
        [ValidateRequest]
        public async Task<IActionResult> EditCategory([FromBody] EditCategoryRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _categoryService.EditCategory(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "user")]
        [HttpPost("products-by-category")]
        [ValidateRequest]
        public async Task<IActionResult> GetProductsByCategory([FromBody] GetProductsByCategoryRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
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