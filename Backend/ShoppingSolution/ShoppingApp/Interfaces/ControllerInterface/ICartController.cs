using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface ICartController
    {
        public Task<ApiResponse<GetCartResponseDTO>> GetCart(GetCartRequestDTO request);
        public Task<ApiResponse<AddToCartResponseDTO>> AddToCart(AddToCartRequestDTO request);
        public Task<ApiResponse<RemoveAllFromCartResponseDTO>> ClearCart();
        public Task<ApiResponse<OrderAllFromCartResponseDTO>> PlaceOrderAllFromCarts(OrderAllFromCartRequestDTO request);
        public Task<ApiResponse<RemoveFromCartResponseDTO>> RemoveFromCart(RemoveFromCartRequestDTO request);
        public Task<ApiResponse<UpdateUserCartResponseDTO>> UpdateUserCart(UpdateUserCartRequestDTO request);
        
    }
}