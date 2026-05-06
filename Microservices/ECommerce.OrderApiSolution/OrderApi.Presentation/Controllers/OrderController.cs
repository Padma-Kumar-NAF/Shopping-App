using ECommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.DTO;
using OrderApi.Application.DTO.Conversion;
using OrderApi.Application.Interfaces;
using OrderApi.Application.Servcies;

namespace OrderApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrder orderInterface,IOrderService orderService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
        {
            var orders = await orderInterface.GetAllAsync();
            if(orders == null)
            {
                return NotFound("No orders found.");
            }

            var (_,list) = OrderConversion.FromEntity(null, orders);
            return !list!.Any() ? NotFound("No orders found.") : Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order = await orderInterface.FindByIdAsync(id);

            if (order is null)
                return NotFound(null);

            var (_order, _) = OrderConversion.FromEntity(order, null);

            return Ok(_order);
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDTO>> GetOrderDetails(int orderId)
        {
            if (orderId <= 0)
                return BadRequest("Invalid data provided");

            var orderDetail = await orderService.GetOrderDetails(orderId);

            return orderDetail.OrderId > 0
                ? Ok(orderDetail)
                : NotFound("No order found");
        }

        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<OrderDTO>> GetClientOrders(int clientId)
        {
            if (clientId <= 0)
                return BadRequest("Invalid data provided");

            var orders = await orderService.GetOrdersByClientId(clientId);

            return !orders.Any() ? NotFound(null) : Ok(orders);
        }

        [HttpPut]
        public async Task<ActionResult<Response>> UpdateOrder(OrderDTO orderDTO)
        {
            var order = OrderConversion.ToEntity(orderDTO);

            var response = await orderInterface.UpdateAsync(order);

            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpDelete]
        public async Task<ActionResult<Response>> DeleteOrder(OrderDTO orderDTO)
        {
            // Convert from DTO to entity
            var order = OrderConversion.ToEntity(orderDTO);

            var response = await orderInterface.DeleteAsync(order);

            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateOrder(OrderDTO orderDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest("Incomplete data submitted");

            var getEntity = OrderConversion.ToEntity(orderDTO);

            var response = await orderInterface.CreateAsync(getEntity);

            return response.Flag ? Ok(response) : BadRequest(response);
        }
    }
}
