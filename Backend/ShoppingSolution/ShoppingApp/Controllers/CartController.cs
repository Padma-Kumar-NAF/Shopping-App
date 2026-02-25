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
    }
}
