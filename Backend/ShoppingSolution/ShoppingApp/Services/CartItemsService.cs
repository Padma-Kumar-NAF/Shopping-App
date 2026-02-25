using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;

namespace ShoppingApp.Services
{
    public class CartItemsService : ICartItemsService
    {
        ICartItemRepository _cartItemRepository;
        private readonly ShoppingContext _context;
        public CartItemsService( ICartItemRepository cartItemRepository, ShoppingContext context)
        {
            _cartItemRepository = cartItemRepository;
            _context = context;
        }

        public async Task<IEnumerable<GetCartResponseDTO>> GetCartItems(GetCartRequestDTO request)
        {
            var items = await _cartItemRepository.GetCartItemsByUserAsync(
                request.UserId,
                request.PageNumber,
                request.Limit);
            return items;
        }

        public async Task<bool> RemoveAllByCartId(Guid cartId)
        {
            var affectedRows = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ExecuteDeleteAsync();

            return affectedRows > 0;
        }
    }
}
