using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Wishlist;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IWishListController
    {
        public Task<ActionResult<ICollection<GetUserWishListResponseDTO>>> GetUserWishList(GetUserWishListRequestDTOClass request);
        public Task<ActionResult<RemoveProductFromWishListResponseDTO>> RemoveFromWishList(RemoveProductFromWishListRequestDTO request);
    }
}