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

        /// <summary>
        /// Cancels an existing order for the authenticated user based on the specified order ID.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized as either an admin
        /// or a user. The request is validated before processing. An exception is thrown if the order ID is invalid or
        /// if the cancellation fails.</remarks>
        /// <param name="request">A request object that contains the identifier of the order to cancel. Must not be null.</param>
        /// <returns>An IActionResult that indicates the outcome of the cancellation operation. Returns a success response if the
        /// order is canceled; otherwise, returns an error response.</returns>
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

        /// <summary>
        /// Retrieves a list of orders for the authenticated user based on the specified filter and pagination criteria.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized with either the
        /// 'admin' or 'user' role.</remarks>
        /// <param name="request">An object containing the filtering and pagination options to apply when retrieving orders.</param>
        /// <returns>An IActionResult containing the collection of orders that match the specified criteria. Returns an error
        /// response if the request is invalid or the user is not authorized.</returns>
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

        /// <summary>
        /// Retrieves the orders associated with the authenticated user.
        /// </summary>
        /// <remarks>This method requires the user to be authenticated. Ensure that the <paramref
        /// name="request"/> parameter is properly populated before calling this method.</remarks>
        /// <param name="request">A request object containing the criteria for fetching the user's orders. Must not be null.</param>
        /// <returns>An <see cref="IActionResult"/> that contains the user's order details if the request is successful;
        /// otherwise, an error response.</returns>
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

        /// <summary>
        /// Processes a refund request for an order using the specified refund details.
        /// </summary>
        /// <remarks>The user must be authenticated to invoke this method. An exception is thrown if the
        /// user identifier cannot be determined or if the refund process encounters an error.</remarks>
        /// <param name="request">An object containing the details of the refund request, including the order identifier and the reason for
        /// the refund. This parameter must not be null.</param>
        /// <returns>An IActionResult that represents the outcome of the refund operation. The result includes the status of the
        /// request and any relevant data related to the refund.</returns>
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

        /// <summary>
        /// Places a new order using the specified order details provided in the request body.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized as either an admin
        /// or user. An exception is thrown if the user ID cannot be retrieved or if the order placement
        /// fails.</remarks>
        /// <param name="request">An object containing the details of the order to be placed, including item identifiers and quantities. This
        /// parameter must not be null.</param>
        /// <returns>An IActionResult that represents the result of the order placement operation. If successful, the response
        /// includes the details of the placed order.</returns>
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

        /// <summary>
        /// Updates the status of an existing order using the specified order information.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized with either the
        /// 'admin' or 'user' role.</remarks>
        /// <param name="request">An object containing the order ID and the new status to apply to the order.</param>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the update operation.</returns>
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