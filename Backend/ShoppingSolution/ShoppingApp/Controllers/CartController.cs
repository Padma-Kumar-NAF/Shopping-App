using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;
using ShoppingApp.Services;
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId!);
        }

        [HttpPost("AddToCart")]
        public async Task<ActionResult<GetCartResponseDTO>> AddToCart([FromBody] AddToCartRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.Cart.UserId = GetUserId();
            try
            {
                var response = await _cartService.AddCart(request);
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            request.UserId = GetUserId();
            var CartId = await _cartService.GetCarts(request.UserId);
            if(CartId == null)
            {
                return BadRequest("No cart items found");
            }
            var result = await _cartItemService.GetCartItems(request);
            return Ok(result);
        }

        [HttpPost("OrderAllFromCart")]
        public async Task<ActionResult<OrderAllFromCartResponseDTO>> PlaceOrderAllFromCarts(OrderAllFromCartRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.UserId = GetUserId();

            if (request.UserId == Guid.Empty)
                return BadRequest("Invalid UserId");

            bool isOrdered = false;
            isOrdered = await _cartService.PlaceOrderAllFromCart(request.CartId, request.UserId,request.AddressId);

            if (!isOrdered)
                return BadRequest("Order failed. Try again.");

            return Ok(new OrderAllFromCartResponseDTO
            {
                IsSuccess = true
            });
        }

        //[Authorize(Roles = "User")]
        [HttpPost("RemoveAllFromCart")]
        public async Task<ActionResult<RemoveAllFromCartResponseDTO>> ClearCart([FromBody] RemoveAllFromCartRequestDTO request)
        {
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
