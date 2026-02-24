using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
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

        [HttpPost("GetUserCart")]
        public async Task<ActionResult<GetCartResponseDTO>> GetCart([FromBody] GetCartRequestDTO request)
        {
            //Console.WriteLine("----------");
            //Console.WriteLine(request.Limit);
            //Console.WriteLine(request.PageNumber);
            //Console.WriteLine(request.UserId);
            //Console.WriteLine("----------");
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
