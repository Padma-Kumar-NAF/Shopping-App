using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Wishlist;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IWishListService
    {
        public Task<ApiResponse<AddProductToWishListResponseDTO>> AddToWishListAsync(Guid UserId, Guid ProductId, Guid WishListId);
        public Task<ApiResponse<CreateWishListResponseDTO>> CreateWishListAsync(string WishListName, Guid UserId);
        public Task<ApiResponse<DeleteWishListResponseDTO>> DeleteWishListAsync(Guid UserId,Guid WishListId);
        public Task<ApiResponse<GetUserWishListResponseDTO>> GetUserWishListAsync(int Limit,int PageNumber,Guid UserId);
        public Task<ApiResponse<RemoveProductFromWishListResponseDTO>> RemoveFromWishListAsync(Guid UserId,Guid WishList,Guid ProductId);
    }
}