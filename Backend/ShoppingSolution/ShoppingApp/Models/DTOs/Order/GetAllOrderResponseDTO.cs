namespace ShoppingApp.Models.DTOs.Order
{
    public record GetAllOrderResponseDTO
    {
        public ICollection<OrderDetailsResponseDTO> Items { get; set; } = new List<OrderDetailsResponseDTO>();
    }
}