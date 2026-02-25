using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public class AddToCartRequestDTO
    {
        [Required]
        public CartDTO Cart { get; set; }

        [Required]
        public ICollection<CartItemsDTO> Items { get; set; }
    }
    public class CartDTO
    {
        public Guid UserId { get; set; }
    }
    public class CartItemsDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
