using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IOrderService
    {
        public Task<ApiResponse<CancelOrderResponseDTO>> CancelOrder(Guid userId, Guid orderId);
        public Task<ApiResponse<GetUserOrderDetailsResponseDTO>> GetUserOrderById(Guid userId,GetUserOrderDetailsRequestDTO request);
        public Task<ApiResponse<PlaceOrderResponseDTO>> PlaceOrder(Guid userId,PlaceOrderRequestDTO request);
        public Task<ApiResponse<UpdateOrderResponseDTO>> UpdateOrder(Guid userId,Guid orderId,string status);
    }
}