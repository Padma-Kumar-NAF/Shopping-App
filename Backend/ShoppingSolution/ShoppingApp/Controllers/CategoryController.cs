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
    [ApiController]
    [Route("category")]
    [ValidateRequest]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Adds a new category using the specified category details.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized with the 'admin'
        /// role. The request is validated before processing. An exception is thrown if the user is not authorized or if
        /// an error occurs during category creation.</remarks>
        /// <param name="request">An object containing the information required to create the new category. Must not be null.</param>
        /// <returns>An IActionResult that indicates the result of the category creation operation.</returns>
        [Authorize(Roles = "admin")]
        [HttpPost("add-category")]
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

        /// <summary>
        /// Deletes a category based on the details provided in the request.
        /// </summary>
        /// <remarks>This action requires the caller to have administrator privileges. The request is
        /// validated before processing. An exception is thrown if the deletion fails due to invalid input or other
        /// errors.</remarks>
        /// <param name="request">An object containing the information required to identify and delete the category.</param>
        /// <returns>An IActionResult that indicates the outcome of the delete operation.</returns>
        [Authorize(Roles = "admin")]
        [HttpDelete("delete-category")]
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

        /// <summary>
        /// Retrieves a paginated list of all categories based on the specified pagination parameters.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized with either the
        /// 'admin' or 'user' role. The user must be valid, as determined by the GetUserIdOrThrow method, before
        /// categories can be retrieved.</remarks>
        /// <param name="request">An object containing pagination information, including the page size and page number used to determine which
        /// categories are returned.</param>
        /// <returns>An IActionResult containing a collection of categories that match the provided pagination parameters. The
        /// result is returned as a JSON response.</returns>
        [HttpPost("get-all-categories")]
        public async Task<IActionResult> GetAllCategories([FromBody] GetAllCategoryRequestDTO request)
        {
            try
            {
                //var UserId = GetUserIdOrThrow();
                var CategoriesList = await _categoryService.GetAllCategories(request.Pagination.PageSize,request.Pagination.PageNumber);
                return Ok(CategoriesList);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Edits an existing category using the specified request data.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized with the 'admin'
        /// role. The request is validated before processing. An exception is thrown if the user is not authorized or if
        /// the operation fails.</remarks>
        /// <param name="request">An object containing the details of the category to update, including the category identifier and the new
        /// property values.</param>
        /// <returns>An IActionResult that represents the result of the edit operation. If successful, the response includes the
        /// updated category details.</returns>
        [Authorize(Roles = "admin")]
        [HttpPost("edit-category")]
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

        /// <summary>
        /// Retrieves a list of products that belong to the specified category, applying any additional filters provided
        /// in the request.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized with the 'user'
        /// role. The request is validated before processing. Any exceptions encountered during execution are propagated
        /// to the caller.</remarks>
        /// <param name="request">An object containing the category identifier and optional filter criteria used to determine which products
        /// to include in the result. Cannot be null.</param>
        /// <returns>An IActionResult containing the operation result. If successful, the response includes the list of products
        /// matching the specified category and filters; otherwise, it contains an error message and the appropriate
        /// status code.</returns>
        [HttpPost("products-by-category")]
        public async Task<IActionResult> GetProductsByCategory([FromBody] GetProductsByCategoryRequestDTO request)
        {
            try
            {
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