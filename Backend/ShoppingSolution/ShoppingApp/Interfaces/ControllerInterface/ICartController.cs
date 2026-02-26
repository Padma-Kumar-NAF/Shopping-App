using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface ICartController
    {
        public Task<ActionResult<GetCartResponseDTO>> GetCart([FromBody] GetCartRequestDTO request);
        public Task<ActionResult<GetCartResponseDTO>> AddToCart([FromBody] AddToCartRequestDTO request);
        public Task<ActionResult<RemoveAllFromCartResponseDTO>> RemoveAllByCartId([FromBody] RemoveAllFromCartRequestDTO request);
        public Task<ActionResult<OrderAllFromCartResponseDTO>> PlaceOrderAllFromCarts(OrderAllFromCartRequestDTO request);
        public Task<ActionResult<RemoveFromCartResponseDTO>> RemoveFromCart(RemoveFromCartRequestDTO request);

    }
}
