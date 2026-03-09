using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Wishlist;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IWishListService
    {
        public Task<bool> DeleteWishListAsync(Guid UserId,Guid WishListId);
        public Task<bool> AddToWishListAsync(Guid UserId,Guid ProductId,Guid WishListId);
        public Task<bool> CreateWishListAsync(string WishListName,Guid UserId);
        public Task<GetUserWishListResponseDTO> GetUserWishListAsync(int Limit,int PageNumber,Guid UserId);
        public Task<bool> RemoveFromWishListAsync(Guid UserId,Guid WishList,Guid ProductId);
        
    }
}