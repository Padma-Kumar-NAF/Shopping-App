using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Wishlist
{
    public record GetUserWishListRequestDTOClass
    {
        public Pagination pagination { get; set; } = new Pagination();
    }
}
