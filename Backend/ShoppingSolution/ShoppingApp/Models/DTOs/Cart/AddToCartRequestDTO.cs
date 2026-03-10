using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Cart
{
    public record AddToCartRequestDTO
    {
        //[Required]
        //public CartDTO Cart { get; set; }
        public Guid UserId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public ICollection<CartItemsDTO> Items { get; set; } = new List<CartItemsDTO>();
    }

    //public class CartDTO
    //{
    //    public Guid UserId { get; set; }
    //}

    public record CartItemsDTO
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Range(0, 1000, ErrorMessage = "Quantity must be between 0 and 1000")]
        public int Quantity { get; set; }
    }
}
