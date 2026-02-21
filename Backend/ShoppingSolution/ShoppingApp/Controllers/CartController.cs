using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Services;

namespace ShoppingApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartItemsService _cartItemService;

        public CartController(ICartItemsService cartItemService)
        {
            _cartItemService = cartItemService;
        }

        [HttpPost("GetUserCart")]
        public async Task<ActionResult<IEnumerable<GetCartResponseDTO>>> GetCart([FromBody] GetCartRequestDTO request)
        {
            var result = await _cartItemService.GetCartItems(request);
            return Ok(result);
        }
    }
}
