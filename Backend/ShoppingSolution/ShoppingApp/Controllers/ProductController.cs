using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ValidateRequest]
    public class ProductsController : BaseController, IProductController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Retrieves a collection of products that match the specified filter and pagination criteria.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized with either the
        /// 'admin' or 'user' role. The request is validated before processing. Any exceptions encountered are
        /// propagated to the caller.</remarks>
        /// <param name="request">An object containing the filtering and pagination options to apply when retrieving products. Cannot be null.</param>
        /// <returns>An IActionResult containing a list of products that satisfy the request criteria. Returns an empty list if
        /// no products are found.</returns>
        [HttpPost("get-products")]
        public async Task<IActionResult> GetProducts([FromBody] GetAllProductsRequestDTO request)
        {
            try
            {
                var Result = await _productService.GetProducts(request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Searches for a product by its name using the specified request data.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized as either an admin
        /// or user. The request is validated before processing. Exceptions encountered during execution are propagated
        /// to the caller.</remarks>
        /// <param name="request">An object containing the criteria for searching products by name. Must not be null.</param>
        /// <returns>An IActionResult containing the search results. Returns a list of matching products if found; otherwise,
        /// returns an empty result or an appropriate error response.</returns>
        [HttpPost("search-product")]
        public async Task<IActionResult> GetProductByName([FromBody] SearchProductByNameRequestDTO request)
        {
            try
            {
                var Result = await _productService.SearchProductByName(request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Adds a new product to the inventory using the specified product details.
        /// </summary>
        /// <remarks>This action requires the caller to be authenticated and authorized with the 'admin'
        /// role. The request is validated before processing, and exceptions may be thrown if validation fails or if the
        /// operation cannot be completed.</remarks>
        /// <param name="request">An object containing the details of the product to add. This includes required attributes such as name,
        /// price, and description.</param>
        /// <returns>An IActionResult that represents the result of the operation. If successful, the response includes the
        /// details of the newly added product.</returns>
        [Authorize(Roles = "admin")]
        [HttpPost("add-product")]
        public async Task<IActionResult> AddProduct([FromBody] AddNewProductRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _productService.AddProduct(UserId,request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the details of an existing product using the specified request data.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized with the appropriate
        /// roles. An exception is thrown if the user is not authorized or if the update operation fails.</remarks>
        /// <param name="request">An <see cref="UpdateProductRequestDTO"/> object containing the updated product information. All required
        /// fields for the update operation must be provided.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the update operation. If successful, the
        /// response includes the updated product details.</returns>
        [Authorize(Roles = "admin")]
        [HttpPost("update-product")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _productService.UpdateProduct(UserId,request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves the details of a product that matches the specified product ID.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized as either an admin
        /// or user. The user's ID is used to perform the search operation.</remarks>
        /// <param name="request">An object containing the product ID to search for. Must not be null.</param>
        /// <returns>An IActionResult containing the product details if found; otherwise, an error response indicating the reason
        /// for failure.</returns>
        [HttpPost("get-product-by-id")]
        public async Task<IActionResult> GetProductById([FromBody] SearchProductByIdRequestDTO request)
        {
            try
            {
                var Result = await _productService.SearchProductById(request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves products based on price filter and pagination.
        /// </summary>
        /// <remarks>
        /// Filters products between a specified price range.
        /// Supports pagination and returns empty result if no products found.
        /// </remarks>
        /// <param name="request">Contains price range and pagination details.</param>
        /// <returns>Filtered list of products.</returns>
        [HttpPost("get-products-with-filter")]
        public async Task<IActionResult> GetProductsWithFilter([FromBody] GetAllProductsWithFilterRequestDTO request)
        {
            try
            {
                var result = await _productService.GetProductsWithFilter(request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }


        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string query)
        {
            try
            {
                var result = await _productService.GetSuggestions(query);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Soft deletes a product by setting its ActiveStatus to false.
        /// </summary>
        /// <remarks>This action requires the caller to be authenticated and authorized with the 'admin' role.</remarks>
        /// <param name="request">An object containing the ProductId to soft delete.</param>
        /// <returns>An IActionResult indicating whether the product was successfully soft deleted.</returns>
        [Authorize(Roles = "admin")]
        [HttpPost("delete-product")]
        public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _productService.DeleteProduct(UserId, request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }
    }
}