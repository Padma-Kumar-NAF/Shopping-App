using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public class DeleteWishListRequestDTO
    {
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Wish list id is required")]
        public Guid WishListId { get; set; }
    }
}
