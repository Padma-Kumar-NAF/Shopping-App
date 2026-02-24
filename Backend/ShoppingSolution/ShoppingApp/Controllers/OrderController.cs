using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase , IOrderController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("GetUserOrders")]
        public async Task<ActionResult<IEnumerable<GetUserOrderDetailsResponseDTO>>> GetOrderByUserId(GetUserOrderDetailsRequestDTO request)
        {
            try
            {
                var orders = await _orderService.GetUserOrderById(request);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("PlaceOrder")]
        public async Task<ActionResult<GetUserOrderDetailsResponseDTO>> PlaceOrder(PlaceOrderRequestDTO request)
        {
            try
            {
                var orders = await _orderService.PlaceOrder(request);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
