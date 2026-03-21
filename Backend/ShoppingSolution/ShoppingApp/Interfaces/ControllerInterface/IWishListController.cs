using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Wishlist;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IWishListController
    {
        public Task<IActionResult> CreateWishList(CreateWishListRequestDTO request);
        public Task<IActionResult> DeleteWishList(DeleteWishListRequestDTO request);
        public Task<IActionResult> AddToWishListAsync(AddProductToWishListRequestDTO request);
        public Task<IActionResult> RemoveFromWishList(RemoveProductFromWishListRequestDTO request);
        public Task<IActionResult> GetUserWishList(GetUserWishListRequestDTOClass request);
    }
}