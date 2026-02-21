using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs
{
    public class GetCartResponseDTO
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid CartId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
