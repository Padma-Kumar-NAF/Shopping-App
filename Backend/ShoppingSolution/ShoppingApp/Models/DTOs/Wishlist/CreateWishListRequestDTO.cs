using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public class CreateWishListRequestDTO
    {
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Wishlist name is required")]
        public string WishListName { get; set; } = string.Empty;
    }
}
