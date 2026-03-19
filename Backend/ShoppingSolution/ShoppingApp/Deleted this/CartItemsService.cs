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
        private readonly IRepository<Guid, CartItem> _repository;

        public CartItemsService(IRepository<Guid, CartItem> repository)
        {
            _repository = repository;
        }

        public async Task<GetCartResponseDTO> GetCartItems(Guid CartId, Guid UserId, int Limit, int PageNumber)
        {
            var query = _repository.GetQueryable()
                .Where(ci => ci.CartId == CartId)
                .Include(ci => ci.Product);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((PageNumber - 1) * Limit)
                .Take(Limit)
                .Select(ci => new CartItemDTO
                {
                    ProductId = ci.ProductId,
                    CategoryId = ci.Product != null ? ci.Product.CategoryId : Guid.Empty,
                    ProductName = ci.Product != null ? ci.Product.Name : string.Empty,
                    ImagePath = ci.Product != null ? ci.Product.ImagePath : string.Empty,
                    Description = ci.Product != null ? ci.Product.Description : string.Empty,
                    Price = ci.Product != null ? ci.Product.Price : 0,
                    Quantity = ci.Quantity
                })
                .ToListAsync();

            return new GetCartResponseDTO
            {
                CartId = CartId,
                CartItems = items
            };
        }

        public async Task<bool> RemoveAllByCartId(Guid cartId)
        {
            var items = await _repository.GetQueryable()
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            if (!items.Any())
                return false;

            foreach (var item in items)
            {
                await _repository.DeleteAsync(item.CartItemId);
            }

            return true;
        }
    }
}
