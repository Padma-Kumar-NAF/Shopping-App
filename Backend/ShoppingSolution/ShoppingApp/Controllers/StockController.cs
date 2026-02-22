using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Product;
using ShoppingApp.Models.DTOs.Stock;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StockController : ControllerBase , IStockController
    {
        private readonly IStockService _stockService;
        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost("GetStock")]
        public async Task<ActionResult<IEnumerable<GetStockResponseDTO>>> GetStock([FromBody] GetStockRequestDTO request)
        {
            try
            {
                var products = await _stockService.GetStock(request);
                if (products == null || !products.Any())
                {
                    return NotFound("No Stocks found.");
                }
                return Ok(products);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
