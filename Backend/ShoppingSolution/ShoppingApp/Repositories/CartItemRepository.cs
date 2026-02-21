using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Repositories
{
    public class CartItemRepository
    : Repository<Guid, CartItem>, ICartItemRepository
    {
        public CartItemRepository(ShoppingContext context) : base(context)
        {

        }

        public async Task<IEnumerable<GetCartResponseDTO>> GetCartItemsByUserAsync(Guid userId,int pageNumber,int pageSize)
        {
            return await _context.CartItems
            .Where(ci => ci.Cart.UserId == userId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ci => new GetCartResponseDTO
            {
                CartId = ci.CartId,
                ProductId = ci.ProductId,
                CategoryId = ci.Product.CategoryId,
                Name = ci.Product.Name,
                ImagePath = ci.Product.ImagePath,
                Description = ci.Product.Description,
                Price = ci.Product.Price,
                Quantity = ci.Quantity
            }).OrderBy(c=>c.Name)
            .ToListAsync();
        }
    }
}
