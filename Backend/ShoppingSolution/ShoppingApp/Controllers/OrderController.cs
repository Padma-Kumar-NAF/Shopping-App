using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Order;
using System.Security.Claims;

namespace ShoppingApp.Controllers
{
    //[Authorize(Roles = "admin,user")]
    [Route("orders")]
    [ApiController]
    public class OrderController : BaseController, IOrderController
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        //[Authorize(Roles = "admin,user")]
        [HttpPost("cancel-order")]
        [ValidateRequest]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _orderService.CancelOrder(UserId,request.OrderId);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin,user")]
        [HttpPost("get-all-orders")]
        [ValidateRequest]
        public async Task<IActionResult> GetAllOrders(GetAllOrderRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _orderService.GetAllOrders(request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        //[HttpGet("user")]
        [HttpPost("get-user-orders")]
        [ValidateRequest]
        public async Task<IActionResult> GetOrderByUserId([FromBody] GetUserOrderDetailsRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _orderService.GetUserOrderById(UserId,request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[HttpGet("user")]
        [HttpPost("refund-order")]
        [ValidateRequest]
        public async Task<IActionResult> OrderRefund([FromBody] OrderRefundRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _orderService.OrderRefund(UserId, request);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin,user")]
        [HttpPost("place-order")]
        [ValidateRequest]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _orderService.PlaceOrder(UserId,request);
                return Ok(result); 
            } 
            catch 
            {
                throw;
            }
        }

        //[Authorize(Roles = "admin,user")]
        [HttpPost("update-order-status")]
        [ValidateRequest]
        public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _orderService.UpdateOrder(UserId,request.OrderId,request.OrderStatus);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}