using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Product;
using System.ComponentModel;

namespace ShoppingApp.Controllers
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase, IProductController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        //[Authorize(Roles = "Admin,User")]
        [HttpPost("getProducts")]
        public async Task<ActionResult<IEnumerable<GetAllProductsResponseDTO>>> GetProducts([FromBody] GetAllProductsRequestDTO request)
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

        //[Authorize(Roles = "Admin,User")]
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<GetAllProductsResponseDTO>>> GetProductByName([FromBody] SearchProductRequestDTO request)
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

        //[Authorize(Roles = "Admin")]
        [HttpPost("AddProduct")]
        public async Task<ActionResult<GetAllProductsResponseDTO>> AddProduct(AddNewProductRequestDTO request)
        {
            try
            {
                var newProduct = await _productService.AddProduct(request);
                return Ok(newProduct);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("UpdateProduct")]
        public async Task<ActionResult<UpdateProductResponseDTO>> UpdateProduct(UpdateProductRequestDTO request)
        {
            try
            {
                var newProduct = await _productService.UpdateProduct(request);
                return Ok(newProduct);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}