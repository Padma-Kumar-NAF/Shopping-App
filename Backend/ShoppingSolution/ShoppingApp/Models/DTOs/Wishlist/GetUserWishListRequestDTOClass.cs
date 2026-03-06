using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public class GetUserWishListRequestDTOClass
    {
        [Required(ErrorMessage = "Limit is required")]
        public int Limit { get; set; }
        [Required(ErrorMessage = "Page number is required")]
        public int PageNumber { get; set; }
        public Guid UserId { get; set; }
    }
}
