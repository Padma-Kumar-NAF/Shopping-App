using Microsoft.AspNetCore.Mvc;
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
    public class CartController : ControllerBase , ICartController
    {
        private readonly ICartItemsService _cartItemService;
        private readonly ICartService _cartService;

        public CartController(ICartItemsService cartItemService, ICartService cartService)
        {
            _cartItemService = cartItemService;
            _cartService = cartService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Guid.Empty;

            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : Guid.Empty;
        }

        [HttpPost("AddToCart")]
        public async Task<ActionResult<GetCartResponseDTO>> AddToCart([FromBody] AddToCartRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request == null)
                return BadRequest("Invalid request");

            //request.Cart.UserId = GetUserId();
            request.UserId = GetUserId();

            //if (request.Cart.UserId == Guid.Empty)
            //    return Unauthorized("User not authenticated");
            
            if (request.UserId == Guid.Empty)
                return Unauthorized("User not authenticated");

            try
            {
                var response = await _cartService.AddCart(request);
                if (response == null)
                    return StatusCode(500, "Unable to update cart");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[Authorize(Roles = "User")]
        [HttpPost("GetUserCart")]
        public async Task<ActionResult<GetCartResponseDTO>> GetCart([FromBody] GetCartRequestDTO request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.UserId = GetUserId();
            Console.WriteLine("request.UserId");
            Console.WriteLine(request.UserId);
            var Cart = await _cartService.GetCarts(request.UserId);

            if(Cart == null)
            {
                return Ok("No cart items found");
            }

            var result = await _cartItemService.GetCartItems(Cart.CartId, request.UserId,request.Limit,request.PageNumber);
            return Ok(result);
        }

        [HttpPost("OrderAllFromCart")]
        public async Task<ActionResult<OrderAllFromCartResponseDTO>> PlaceOrderAllFromCarts([FromBody] OrderAllFromCartRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.UserId = GetUserId();

            if (request.UserId == Guid.Empty)
                return BadRequest("Invalid user.");

            if (request.CartId == Guid.Empty || request.AddressId == Guid.Empty)
                return BadRequest("CartId and AddressId are required.");

            var isOrdered = await _cartService.PlaceOrderAllFromCart(
                request.CartId,
                request.UserId,
                request.AddressId);

            if (!isOrdered)
                return BadRequest("Order failed. Please check cart, stock, or address.");

            return Ok(new OrderAllFromCartResponseDTO
            {
                IsSuccess = true
            });
        }

        //[Authorize(Roles = "User")]
        [HttpPost("RemoveAllFromCart")]
        public async Task<ActionResult<RemoveAllFromCartResponseDTO>> ClearCart([FromBody] RemoveAllFromCartRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (request.UserId == Guid.Empty)
                return BadRequest("Invalid UserId");

            var flag = await _cartService.RemoveAllFromCartByUserID(request.UserId);

            if (!flag)
                return NotFound(new RemoveAllFromCartResponseDTO
                {
                    IsRemoved = false,
                    Message = "Cart not found or already empty."
                });

            return Ok(new RemoveAllFromCartResponseDTO
            {
                IsRemoved = true,
                Message = "All items removed successfully."
            });
        }

        [HttpDelete("RemoveFromCart")]
        public async Task<ActionResult<RemoveFromCartResponseDTO>> RemoveFromCart( [FromBody] RemoveFromCartRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.UserId = GetUserId();

            if(request.UserId == Guid.Empty)
            {
                return BadRequest("User not found");
            }

            // Check the user have a cart or not
            var result = await _cartService.RemoveFromCart(request.CartId, request.ProductId);

            if (!result)    
            {
                return NotFound(new RemoveFromCartResponseDTO
                {
                    Success = false,
                    Message = "Product not found in cart"
                });
            }

            return Ok(new RemoveFromCartResponseDTO
            {
                Success = true,
                Message = "Product removed successfully"
            });
        }
    }
}
