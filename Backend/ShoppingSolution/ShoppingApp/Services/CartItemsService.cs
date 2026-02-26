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
        private readonly ShoppingContext _context;
        public CartItemsService(ShoppingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GetCartResponseDTO>> GetCartItems(GetCartRequestDTO request)
        {
            return await _context.Carts
                .Where(c => c.UserId == request.UserId)
                .Skip((request.PageNumber - 1) * request.Limit)
                .Take(request.Limit)
                .Select(c => new GetCartResponseDTO
                {
                    CartId = c.CartId,
                    UserId = c.UserId,
                    Items = c.CartItems!.Select(ci => new CartItemDTO
                    {
                        ProductId = ci.ProductId,
                        CategoryId = ci.Product!.CategoryId,
                        ProductName = ci.Product.Name,
                        ImagePath = ci.Product.ImagePath,
                        Description = ci.Product.Description,
                        Price = ci.Product.Price,
                        Quantity = ci.Quantity
                    }).ToList()
                })
                .ToListAsync();
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
