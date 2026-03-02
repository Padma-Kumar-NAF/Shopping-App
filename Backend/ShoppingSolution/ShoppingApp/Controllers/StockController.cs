//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using ShoppingApp.Interfaces.ControllerInterface;
//using ShoppingApp.Interfaces.ServicesInterface;
//using ShoppingApp.Models.DTOs.Product;
//using ShoppingApp.Models.DTOs.Stock;
//using ShoppingApp.Services;
//using System.Security.Claims;

//namespace ShoppingApp.Controllers
//{
//    //[Authorize(Roles = "Admin")]
//    [Route("[controller]")]
//    [ApiController]
//    public class StockController : ControllerBase , IStockController
//    {
//        private readonly IStockService _stockService;
//        public StockController(IStockService stockService)
//        {
//            _stockService = stockService;
//        }

//        private Guid GetUserId()
//        {
//            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

//            if (string.IsNullOrWhiteSpace(userIdClaim))
//                return Guid.Empty;

//            return Guid.TryParse(userIdClaim, out var userId)
//                ? userId
//                : Guid.Empty;
//        }


//        // No need to add a stock in this way , the product service will add the stock automatically
//        [HttpPost("AddNewStock")]
//        public async Task<ActionResult<AddNewStockResponseDTO>> AddStock(AddNewStockRequestDTO request)
//        {
//            try
//            {
//                var result = await _stockService.AddStock(request);
//                return Ok(result);
//            }
//            catch (Exception e)
//            {
//                return BadRequest(e.Message);
//            }
//        }


//        // 
//        [HttpPost("GetStock")]
//        public async Task<ActionResult<IEnumerable<GetStockResponseDTO>>> GetStock([FromBody] GetStockRequestDTO request)
//        {
//            try
//            {
//                var products = await _stockService.GetStock(request);
//                if (products == null || !products.Any())
//                {
//                    return NotFound("No Stocks found.");
//                }
//                return Ok(products);
//            }
//            catch (Exception e)
//            {
//                return BadRequest(e.Message);
//            }
//        }
//    }
//}
