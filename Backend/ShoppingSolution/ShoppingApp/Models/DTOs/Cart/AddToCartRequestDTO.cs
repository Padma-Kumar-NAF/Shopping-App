using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public record AddToCartRequestDTO
    {

        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Range(0, 1000, ErrorMessage = "Quantity must be between 0 and 1000")]
        public int Quantity { get; set; }
    }
}
