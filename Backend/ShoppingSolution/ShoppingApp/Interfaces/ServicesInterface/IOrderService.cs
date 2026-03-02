using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IOrderService
    {
        public Task<IEnumerable<GetUserOrderDetailsResponseDTO>> GetUserOrderById(GetUserOrderDetailsRequestDTO request);
        public Task<GetUserOrderDetailsResponseDTO> PlaceOrder(PlaceOrderRequestDTO request);
        public Task<GetUserOrderDetailsResponseDTO> CancelOrder(CancelOrderRequestDTO request);
        public Task<bool> UpdateOrder(Guid OrderId,string Status);
    }
}
