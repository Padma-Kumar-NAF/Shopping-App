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

        //public async Task<IEnumerable<GetCartResponseDTO>> GetCartItems(GetCartRequestDTO request)
        //{
        //    return await _context.Carts
        //        .Where(c => c.UserId == request.UserId)
        //        .Skip((request.PageNumber - 1) * request.Limit)
        //        .Take(request.Limit)
        //        .Select(c => new GetCartResponseDTO
        //        {
        //            CartId = c.CartId,
        //            UserId = c.UserId,
        //            Items = c.CartItems!.Select(ci => new CartItemDTO
        //            {
        //                ProductId = ci.ProductId,
        //                CategoryId = ci.Product!.CategoryId,
        //                ProductName = ci.Product.Name,
        //                ImagePath = ci.Product.ImagePath,
        //                Description = ci.Product.Description,
        //                Price = ci.Product.Price,
        //                Quantity = ci.Quantity
        //            }).ToList()
        //        })
        //        .ToListAsync();
        //}

        public async Task<GetCartResponseDTO> GetCartItems(Guid CartId,Guid UserId,int Limit,int PageNumber)
        {
            //var cart = await _context.Carts
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(c => c.UserId == request.UserId);

            //if (cart == null)
            //{
            //    return new GetCartResponseDTO
            //    {
            //        CartId = Guid.Empty,
            //        UserId = request.UserId,
            //        Items = new List<CartItemDTO>()
            //    };
            //}

            var query = _context.CartItems
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
                UserId = UserId,
                Items = items,
            };
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
