using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Cart;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "user")]
    [Route("[controller]")]
    [ApiController]
    [ValidateRequest]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [Authorize(Roles = "user")]
        [HttpPost("add-to-cart")]
        [ValidateRequest]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var response = await _cartService.AddCart(UserId,request);
                return Ok(response);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost("get-user-cart")]
        [ValidateRequest]
        public async Task<IActionResult> GetCart([FromBody] GetCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.GetUserCarts(UserId, request.Pagination.PageNumber, request.Pagination.PageSize);

                return BadRequest(result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// If user Increase or decrease the quantity from cart age page then this controller should be call
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "user")]
        [HttpPost("update-user-cart")]
        [ValidateRequest]
        public async Task<IActionResult> UpdateUserCart([FromBody] UpdateUserCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.UpdateCart(UserId,request.CartId, request.CartItemId, request.ProductId, request.Quantity);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost("order-all-from-cart")]
        [ValidateRequest]
        public async Task<IActionResult> PlaceOrderAllFromCarts([FromBody] OrderAllFromCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.PlaceOrderAllFromCart(UserId,request.AddressId,request.PaymentType);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost("remove-all-from-cart")]
        [ValidateRequest]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.RemoveAllFromCartByUserID(UserId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpDelete("remove-from-cart")]
        [ValidateRequest]
        public async Task<IActionResult> RemoveFromCart( [FromBody] RemoveFromCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.RemoveFromCart(UserId, request.CartId, request.CartItemId,request.ProductId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}