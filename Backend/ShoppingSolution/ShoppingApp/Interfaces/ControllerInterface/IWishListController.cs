using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Wishlist;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IWishListController
    {
        public Task<ActionResult<CreateWishListResponseDTO>> CreateWishList(CreateWishListRequestDTO request);
        public Task<ActionResult<DeleteWishListResponseDTO>> DeleteWishList(DeleteWishListRequestDTO request);
        public Task<ActionResult<AddProductToWishListResponseDTO>> AddToWishListAsync(AddProductToWishListRequestDTO request);
        public Task<ActionResult<RemoveProductFromWishListResponseDTO>> RemoveFromWishList(RemoveProductFromWishListRequestDTO request);
        public Task<ActionResult<GetUserWishListResponseDTO>> GetUserWishList(GetUserWishListRequestDTOClass request);
    }
}