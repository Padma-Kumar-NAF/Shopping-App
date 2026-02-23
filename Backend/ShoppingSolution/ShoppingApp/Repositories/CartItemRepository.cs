using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Repositories
{
    public class CartItemRepository : Repository<Guid, CartItem>, ICartItemRepository
    {
        public CartItemRepository(ShoppingContext context) : base(context)
        {

        }

        public async Task<IEnumerable<GetCartResponseDTO>> GetCartItemsByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
    }
}
