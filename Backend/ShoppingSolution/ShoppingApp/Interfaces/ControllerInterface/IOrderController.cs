using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IOrderController
    {
        public Task<IActionResult> GetOrderByUserId(GetUserOrderDetailsRequestDTO request);
        public Task<IActionResult> PlaceOrder(PlaceOrderRequestDTO request);
        public Task<IActionResult> CancelOrder(CancelOrderRequestDTO request);
        public Task<IActionResult> UpdateOrder(UpdateOrderRequestDTO request);
    }
}
