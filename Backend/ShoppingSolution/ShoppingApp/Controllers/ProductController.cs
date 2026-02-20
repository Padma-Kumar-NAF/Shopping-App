using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Controllers
{
    [Route("products")]
    [ApiController]
    public class ProductsController : ControllerBase , IProductController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService userService)
        {
            _productService = userService;
        }

        [HttpPost("getAllProducts")]
        public async Task<ActionResult<IEnumerable<GetAllProductsResponse>>> GetAll(GetAllProductsRequest request)
        {
            try
            {
                var product = await _productService.GetAllProducts();
                return Ok(product);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
