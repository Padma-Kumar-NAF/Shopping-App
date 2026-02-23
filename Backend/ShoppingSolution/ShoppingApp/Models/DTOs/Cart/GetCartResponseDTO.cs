using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public class GetCartResponseDTO
    {
        // Add collection of cartitems
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        [Required]
        public Guid CartId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string ImagePath { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}