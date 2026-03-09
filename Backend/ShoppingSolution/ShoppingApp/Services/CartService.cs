using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;

namespace ShoppingApp.Services
{
    public class CartService : ICartService
    {
        private readonly ShoppingContext _context;
        //private readonly ICartItemsService _cartItemsService;
        public CartService(ShoppingContext context, ICartItemsService cartItemsService)
        {
            _context = context;
            //_cartItemsService = cartItemsService;
        }

        public async Task<GetCartResponseDTO?> AddCart(AddToCartRequestDTO request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                throw new ArgumentException("Cart items cannot be empty");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == request.UserId);

                if (cart == null)
                {
                    cart = new Cart { UserId = request.UserId };
                    await _context.Carts.AddAsync(cart);
                    await _context.SaveChangesAsync();
                }

                var existingItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .ToListAsync();

                foreach (var item in request.Items)
                {
                    if (item.ProductId == Guid.Empty)
                        continue;

                    // 🔹 Check product exists
                    var productExists = await _context.Products
                        .AnyAsync(p => p.ProductId == item.ProductId);

                    if (!productExists)
                        throw new Exception($"Product not found for ProductId: {item.ProductId}");

                    var existingItem = existingItems
                        .FirstOrDefault(ci => ci.ProductId == item.ProductId);

                    if (existingItem != null)
                    {
                        if (item.Quantity <= 0)
                            _context.CartItems.Remove(existingItem);
                        else
                            existingItem.Quantity = item.Quantity;
                    }
                    else if (item.Quantity > 0)
                    {
                        await _context.CartItems.AddAsync(new CartItem
                        {
                            CartId = cart.CartId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await BuildCartResponse(cart.CartId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Cart?> GetCarts(Guid userId)
        {
            return await _context.Carts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> PlaceOrderAllFromCart(Guid cartId, Guid userId,Guid addressId,string PaymentType)
        {
            if (cartId == Guid.Empty || userId == Guid.Empty || addressId == Guid.Empty)
                return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems!)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return false;

                var addressExists = await _context.Addresses.AnyAsync(a => a.AddressId == addressId && a.UserId == userId);

                if (!addressExists)
                    return false;

                var cartItems = cart.CartItems!.ToList();
                var productIds = cartItems.Select(ci => ci.ProductId).ToList();

                var stocks = await _context.Stock
                    .Where(s => productIds.Contains(s.ProductId))
                    .ToListAsync();


                foreach (var item in cartItems)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);

                    if (stock == null)
                        throw new Exception($"Stock not found for product {item.Product!.Name}");

                    if (stock.Quantity < item.Quantity)
                        throw new Exception($"Insufficient stock for product {item.Product!.Name}");

                    if (item.Product == null)
                        throw new Exception($"Product Not Found");
                }


                var order = new Order
                {
                    UserId = userId,
                    Status = "Not Delivered",
                    TotalProductsCount = cartItems.Sum(x => x.Quantity),
                    TotalAmount = cartItems.Sum(x => x.Quantity * x.Product!.Price),
                    AddressId = addressId,
                    DeliveryDate = DateTime.Now.AddDays(2),
                    OrderDetails = new List<OrderDetails>()
                };

                foreach (var item in cartItems)
                {
                    var stock = stocks.First(s => s.ProductId == item.ProductId);
                    
                    if (stock == null || item.Product == null)
                        return false;

                    stock.Quantity -= item.Quantity;

                    order.OrderDetails!.Add(new OrderDetails
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product!.Name,
                        ProductPrice = item.Product.Price,
                        Quantity = item.Quantity
                    });
                }

                await _context.Orders.AddAsync(order);

                var payment = new Payment()
                {
                    UserId = userId,
                    OrderId = order.OrderId,
                    TotalAmount = cartItems.Sum(x => x.Quantity * x.Product!.Price),
                    PaymentType = PaymentType
                };

                await _context.Payments.AddAsync(payment);

                _context.CartItems.RemoveRange(cartItems);

                _context.Carts.Remove(cart);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> RemoveAllFromCartByUserID(Guid userId)
        {
            var cartId = await _context.Carts
                .Where(c => c.UserId == userId)
                .Select(c => c.CartId)
                .FirstOrDefaultAsync();

            if (cartId == Guid.Empty)
                return false;

            await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ExecuteDeleteAsync();

            var affectedRowsInCart = await _context.Carts
                .Where(c => c.UserId == userId)
                .ExecuteDeleteAsync();

            return affectedRowsInCart > 0;
        }


        // If user removes all items from Cart items then remove the cart from Cart table
        public async Task<bool> RemoveFromCart(Guid cartId, Guid productId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);

            if (cartItem == null)
                return false;

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<GetCartResponseDTO> BuildCartResponse(Guid cartId)
        {
            return await _context.Carts
                .Where(c => c.CartId == cartId)
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
                .FirstAsync();
        }
    }
}