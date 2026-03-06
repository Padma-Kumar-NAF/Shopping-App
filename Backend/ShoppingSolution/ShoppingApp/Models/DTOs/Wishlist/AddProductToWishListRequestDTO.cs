using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public class AddProductToWishListRequestDTO
    {
        public Guid UserId { get; set; }
        [Required(ErrorMessage ="Product Id is required")]
        public Guid ProductId { get; set; }
        [Required(ErrorMessage = "WishList Id is required")]
        public Guid WishListId { get; set; }
    }
}
