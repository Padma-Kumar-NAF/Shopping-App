using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public record DeleteWishListRequestDTO
    {
        [Required(ErrorMessage = "Wish list id is required")]
        public Guid WishListId { get; set; }
    }
}