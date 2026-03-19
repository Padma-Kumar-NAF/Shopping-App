using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public record RemoveFromCartRequestDTO
    {
        [Required(ErrorMessage = "Cart Id is required")]
        public Guid CartId { get; set; }

        [Required(ErrorMessage = "Cart item Id is required")]
        public Guid CartItemId { get; set; }

        [Required(ErrorMessage = "Product Id is required")]
        public Guid ProductId { get; set; }
    }
}