using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public record DeleteWishListRequestDTO
    {
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Wish list id is required")]
        public Guid WishListId { get; set; }
    }
}
