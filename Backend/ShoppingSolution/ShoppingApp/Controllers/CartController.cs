using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;
using ShoppingApp.Services;

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

        [HttpPost("AddToCart")]
        public async Task<ActionResult<GetCartResponseDTO>> AddToCart([FromBody] AddToCartRequestDTO request)
        {
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
            if (request.UserId == Guid.Empty)
                return BadRequest("Invalid UserId");

            bool isOrdered = false;
            isOrdered = await _cartService.PlaceOrderAllFromCart(request.CartId, request.UserId,request.AddressId);

            if (!isOrdered)
            {
                return BadRequest("Try again !");
            }
            return Ok(new OrderAllFromCartResponseDTO()
            {
                IsSuccess = isOrdered
            });
        }

        //[Authorize(Roles = "User")]
        [HttpPost("RemoveAllFromCart")]
        public async Task<ActionResult<RemoveAllFromCartResponseDTO>> RemoveAllByCartId([FromBody] RemoveAllFromCartRequestDTO request)
        {
            if (request.UserId == Guid.Empty)
                return BadRequest("Invalid UserId");

            var flag = await _cartService.RemoveAllFromCartByUserID(request.UserId);

            var response = new RemoveAllFromCartResponseDTO
            {
                IsRemoved = flag,
                Message = flag
                    ? "All items removed successfully."
                    : "Cart not found or already empty."
            };

            if (!flag)
                return NotFound(response);

            return Ok(response);
        }
    }
}
