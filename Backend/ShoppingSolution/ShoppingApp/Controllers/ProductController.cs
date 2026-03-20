using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Controllers
{
    //[Authorize(Roles = "admin,user")]
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : BaseController, IProductController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        //[Authorize(Roles = "admin,user")]
        [HttpPost("get-products")]
        [ValidateRequest]
        public async Task<IActionResult> GetProducts([FromBody] GetAllProductsRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _productService.GetProducts(request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin,user")]
        [HttpPost("search-product")]
        [ValidateRequest]
        public async Task<IActionResult> GetProductByName([FromBody] SearchProductByNameRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _productService.SearchProductByName(request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin")]
        [HttpPost("add-product")]
        [ValidateRequest]
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

        //[Authorize(Roles = "admin,user")]
        [HttpPost("update-product")]
        [ValidateRequest]
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

        //[Authorize(Roles = "admin,user")]
        [HttpPost("get-product-by-id")]
        [ValidateRequest]
        public async Task<IActionResult> GetProductById([FromBody] SearchProductByIdRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _productService.SearchProductById(request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }
    }
}