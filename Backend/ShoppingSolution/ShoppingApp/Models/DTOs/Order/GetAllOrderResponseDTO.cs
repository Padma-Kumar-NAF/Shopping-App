namespace ShoppingApp.Models.DTOs.Order
{
    public class GetAllOrderResponseDTO
    {
        public ICollection<OrderDetailsResponseDTO> Items { get; set; } = new List<OrderDetailsResponseDTO>();
    }
}