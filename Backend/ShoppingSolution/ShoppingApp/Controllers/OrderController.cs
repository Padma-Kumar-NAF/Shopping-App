using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("GetUserOrders")]
        public async Task<ActionResult<IEnumerable<GetUserOrderDetailsResponseDTO>>> GetOrderByUserId(GetUserOrderDetailsRequestDTO request)
        {
            var orders = await _orderService.GetUserOrderById(request);
            return Ok(orders);
        }
    }
}
