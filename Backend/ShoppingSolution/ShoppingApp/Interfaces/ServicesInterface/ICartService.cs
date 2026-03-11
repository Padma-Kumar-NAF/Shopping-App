using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.Cart;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICartService
    {
        public Task<Cart?> GetCarts(Guid UserId);
        public Task<GetCartResponseDTO?> AddCart(AddToCartRequestDTO UserId);
        public Task<bool> RemoveAllFromCartByUserID(Guid UserId);
        public Task<bool> PlaceOrderAllFromCart(Guid CartId,Guid UserId,Guid AddressId,string PaymentType);
        public Task<bool> RemoveFromCart(Guid UserId ,Guid CartId,Guid ProductId);
    }
}
