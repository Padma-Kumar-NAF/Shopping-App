using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.Cart;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICartService
    {
        public Task<ApiResponse<GetCartResponseDTO>> GetUserCarts(Guid UserId,int PageNumber, int PageSize);
        public Task<ApiResponse<AddToCartResponseDTO>> AddCart(Guid UserId,AddToCartRequestDTO request);
        public Task<ApiResponse<RemoveAllFromCartResponseDTO>> RemoveAllFromCartByUserID(Guid UserId);
        public Task<ApiResponse<OrderAllFromCartResponseDTO>> PlaceOrderAllFromCart(Guid UserId, Guid AddressId, string PaymentType, string PromoCode, bool UseWallet);
        public Task<ApiResponse<RemoveFromCartResponseDTO>> RemoveFromCart(Guid UserId ,Guid CartId,Guid CatrItemId,Guid ProductId);
        public Task<ApiResponse<UpdateUserCartResponseDTO>> UpdateCart(Guid userId, Guid cartId, Guid cartItemId, Guid productId, int quantity);
    }
}