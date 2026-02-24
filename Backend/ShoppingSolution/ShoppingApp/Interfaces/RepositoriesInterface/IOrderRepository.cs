using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface IOrderRepository
    {
        public Task<GetUserOrderDetailsResponseDTO> PlaceOrder(PlaceOrderRequestDTO request);
    }
}
