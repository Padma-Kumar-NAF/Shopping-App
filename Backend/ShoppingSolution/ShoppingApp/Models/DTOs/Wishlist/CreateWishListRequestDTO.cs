using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public record CreateWishListRequestDTO
    {
        [Required(ErrorMessage = "Wishlist name is required")]
        public string WishListName { get; set; } = string.Empty;
    }
}
