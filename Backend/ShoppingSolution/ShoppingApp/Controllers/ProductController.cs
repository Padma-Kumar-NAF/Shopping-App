using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Controllers
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : BaseController, IProductController
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var products = await _productService.GetProducts(request);
                if (products == null || !products.Any())
                {
                    return NotFound("No products found.");
                }
                return Ok(products);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "Admin,User")]
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<GetAllProductsResponseDTO>>> GetProductByName([FromBody] SearchProductRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                if (request == null)
                    return BadRequest("Invalid request.");

                var products = await _productService.SearchProductByName(request);

                if (products == null || !products.Any())
                    return NotFound("No products found.");

                return Ok(products);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("AddProduct")]
        public async Task<ActionResult<GetAllProductsResponseDTO>> AddProduct(AddNewProductRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var newProduct = await _productService.AddProduct(request);
                return Ok(newProduct);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("UpdateProduct")]
        public async Task<ActionResult<UpdateProductResponseDTO>> UpdateProduct(UpdateProductRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var newProduct = await _productService.UpdateProduct(request);
                return Ok(newProduct);
            }
            catch
            {
                throw;
                //return BadRequest(e.Message);
            }
        }

        [HttpPost("GetProductById")]
        public async Task<ActionResult<GetAllProductsResponseDTO>> GetProductById(SearchProductByIdRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var product = await _productService.SearchProductById(request);
                return Ok(product);
            }
            catch
            {
                //return BadRequest(e.Message);
                throw;
            }
        }
    }
}