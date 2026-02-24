using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IOrderService
    {
        // place order -> Add
        // Cancel order -> delete
        // get orders by id -> Queryable

        public Task<IEnumerable<GetUserOrderDetailsResponseDTO>> GetUserOrderById(GetUserOrderDetailsRequestDTO request);
        public Task<GetUserOrderDetailsResponseDTO> PlaceOrder(PlaceOrderRequestDTO request);
        //public Task<> CancelOrder();
    }
}
