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
                var products = await _productService.GetProducts(request);
                return Ok(products);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin,user")]
        [HttpPost("search-product")]
        [ValidateRequest]
        public async Task<IActionResult> GetProductByName([FromBody] SearchProductRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _productService.SearchProductByName(request);
                return Ok(result);
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
                var newProduct = await _productService.AddProduct(request);
                return Ok(newProduct);
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
                var newProduct = await _productService.UpdateProduct(request);
                return Ok(newProduct);
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
                var product = await _productService.SearchProductById(request);
                return Ok(product);
            }
            catch
            {
                throw;
            }
        }
    }
}