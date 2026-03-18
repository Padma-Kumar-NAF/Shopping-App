using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using System.Security.Claims;

namespace ShoppingApp.Controllers
{
    //[Authorize]
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

        //[Authorize(Roles = "User")]
        
        [HttpPost("AddToCart")]
        [ValidateRequest]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDTO request)
        {
            Guid UserId = GetUserId();
            
            if (UserId == Guid.Empty)
                return Unauthorized("User not authenticated");

            try
            {
                var response = await _cartService.AddCart(UserId,request);
                return Ok(response);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "User")]
        [HttpPost("GetUserCart")]
        [ValidateRequest]
        public async Task<IActionResult> GetCart([FromBody] GetCartRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                var result = await _cartService.GetUserCarts(UserId, request.Pagination.PageNumber, request.Pagination.PageSize);

                if (result.StatusCode == 200)
                    return Ok(result);

                if (result.StatusCode == 404)
                    return NotFound(result);

                return BadRequest(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "User")]
        [HttpPost("UpdateUserCart")]
        [ValidateRequest]
        public async Task<IActionResult> UpdateUserCart(UpdateUserCartRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                

                //if (result.StatusCode == 200)
                    return Ok();

                //if (result.StatusCode == 404)
                //    return NotFound(result);

                //return BadRequest(result);
            }
            catch
            {
                throw;
            }
        }

        
        [HttpPost("OrderAllFromCart")]
        [ValidateRequest]
        public async Task<IActionResult> PlaceOrderAllFromCarts([FromBody] OrderAllFromCartRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                    return BadRequest("Invalid user.");

                if (request.PaymentType == "")
                    return BadRequest("Payment type required");

                var result = await _cartService.PlaceOrderAllFromCart(UserId,request.AddressId,request.PaymentType);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "User")]
        [HttpPost("RemoveAllFromCart")]
        [ValidateRequest]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                    return BadRequest("Invalid UserId");

                var result = await _cartService.RemoveAllFromCartByUserID(UserId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        //[Authorize(Roles = "User")]
        [HttpDelete("RemoveFromCart")]
        [ValidateRequest]
        public async Task<IActionResult> RemoveFromCart( [FromBody] RemoveFromCartRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                {
                    return BadRequest("User not found");
                }

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