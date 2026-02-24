using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IOrderController
    {
        public Task<ActionResult<IEnumerable<GetUserOrderDetailsResponseDTO>>> GetOrderByUserId(GetUserOrderDetailsRequestDTO request);
        public Task<ActionResult<GetUserOrderDetailsResponseDTO>> PlaceOrder(PlaceOrderRequestDTO request);
        public Task<ActionResult<GetUserOrderDetailsResponseDTO>> CancelOrder(CancelOrderRequestDTO request);
    }
}
