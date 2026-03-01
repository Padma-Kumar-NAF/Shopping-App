using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Controllers
{
    //[Authorize(Roles = "Admin,User")]
    [Route("orders")]
    [ApiController]
    public class OrderController : ControllerBase, IOrderController
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPut("CancelOrder")]
        public async Task<ActionResult<GetUserOrderDetailsResponseDTO>> CancelOrder(CancelOrderRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _orderService.CancelOrder(request);

                if (result == null)
                    return NotFound("Order not found or already cancelled.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpGet("user")]
        [HttpPost("GetUserOrders")]
        public async Task<ActionResult<IEnumerable<GetUserOrderDetailsResponseDTO>>> GetOrderByUserId([FromBody] GetUserOrderDetailsRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.PageNumber <= 0 || request.Limit <= 0)
                return BadRequest("Invalid pagination values.");

            try
            {
                var orders = await _orderService.GetUserOrderById(request);

                if (!orders.Any())
                    return NotFound("No orders found.");
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("PlaceOrder")]
        public async Task<ActionResult<GetUserOrderDetailsResponseDTO>> PlaceOrder([FromBody] PlaceOrderRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var orders = await _orderService.PlaceOrder(request);
                return Ok(orders); 
            } catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            } 
        }

        [HttpPost("UpdateOrderStatus")]
        public async Task<ActionResult<UpdateOrderResponseDTO>> UpdateOrder(UpdateOrderRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var orders = await _orderService.UpdateOrder(request.OrderId,request.OrderStatus);
                return Ok(new UpdateOrderResponseDTO()
                {
                    IsUpdated = orders
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}