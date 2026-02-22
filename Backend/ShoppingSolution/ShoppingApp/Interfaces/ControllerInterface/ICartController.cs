using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface ICartController
    {
        public Task<ActionResult<IEnumerable<GetStockResponseDTO>>> GetCart([FromBody] GetCartRequestDTO request);
    }
}
