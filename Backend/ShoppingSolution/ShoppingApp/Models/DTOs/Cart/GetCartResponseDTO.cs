using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public record GetCartResponseDTO
    {
        public Guid CartId { get; set; }
        public ICollection<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
    }

    public record CartItemDTO
    {
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        [Required]
        public string ProductName { get; set; } = string.Empty;
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