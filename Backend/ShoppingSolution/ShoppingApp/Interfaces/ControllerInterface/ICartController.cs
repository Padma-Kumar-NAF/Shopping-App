using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface ICartController
    {
        public Task<ActionResult<IEnumerable<GetCartResponseDTO>>> GetCart([FromBody] GetCartRequestDTO request);
    }
}
