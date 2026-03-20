using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public record AddProductToWishListRequestDTO
    {
        [Required(ErrorMessage ="Product Id is required")]
        public Guid ProductId { get; set; }
        [Required(ErrorMessage = "WishList Id is required")]
        public Guid WishListId { get; set; }
    }
}
