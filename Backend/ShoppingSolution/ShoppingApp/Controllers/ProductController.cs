using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using System.ComponentModel;

namespace ShoppingApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase, IProductController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("getProducts")]
        public async Task<ActionResult<IEnumerable<GetAllProductsResponse>>> GetProducts([FromBody] GetAllProductsRequest request)
        {
            try
            {
                var products = await _productService.GetProducts(request);
                if (products == null || !products.Any())
                {
                    return NotFound("No products found.");
                }
                return Ok(products);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<GetAllProductsResponse>>> GetProductByName([FromBody] SearchProductRequestDTO request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request.");

                var products = await _productService.SearchProducts(request);

                if (products == null || !products.Any())
                    return NotFound("No products found.");

                return Ok(products);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }
    }
}