using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;

namespace ShoppingApp.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<Guid, Cart> _repository;
        private readonly IRepository<Guid, CartItem> _cartItemRepository;
        private readonly IRepository<Guid, Product> _productRepository;
        private readonly IRepository<Guid, Stock> _stockRepository;
        private readonly IRepository<Guid, Order> _orderRepository;
        private readonly IRepository<Guid, Payment> _paymentRepository;
        private readonly IRepository<Guid, Address> _addressRepository;

        private readonly IUnitOfWork _unitOfWork;

        public CartService(
            IRepository<Guid, Cart> repository,
            IRepository<Guid, CartItem> cartItemRepository,
            IRepository<Guid, Product> productRepository,
            IRepository<Guid, Stock> stockRepository,
            IRepository<Guid, Order> orderRepository,
            IRepository<Guid, Payment> paymentRepository,
            IRepository<Guid, Address> addressRepository,
            IUnitOfWork unitOfWork)
            {
                _repository = repository;
                _cartItemRepository = cartItemRepository;
                _productRepository = productRepository;
                _stockRepository = stockRepository;
                _orderRepository = orderRepository;
                _paymentRepository = paymentRepository;
                _addressRepository = addressRepository;
                _unitOfWork = unitOfWork;
            }

        public async Task<GetCartResponseDTO?> AddCart(AddToCartRequestDTO request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                throw new ArgumentException("Cart items cannot be empty");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _repository.GetQueryable()
                    .FirstOrDefaultAsync(c => c.UserId == request.UserId);

                if (cart == null)
                {
                    cart = new Cart { UserId = request.UserId };
                    await _repository.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }

                var existingItems = await _cartItemRepository.GetQueryable()
                    .Where(ci => ci.CartId == cart.CartId)
                    .ToListAsync();

                foreach (var item in request.Items)
                {
                    if (item.ProductId == Guid.Empty)
                        continue;

                    var productExists = await _productRepository.GetQueryable()
                        .AnyAsync(p => p.ProductId == item.ProductId);

                    if (!productExists)
                        throw new Exception($"Product not found: {item.ProductId}");

                    var existingItem = existingItems
                        .FirstOrDefault(ci => ci.ProductId == item.ProductId);

                    if (existingItem != null)
                    {
                        if (item.Quantity <= 0)
                            await _cartItemRepository.DeleteAsync(existingItem.CartItemId);
                        else
                        {
                            existingItem.Quantity = item.Quantity;
                            await _cartItemRepository.UpdateAsync(existingItem.CartItemId, existingItem);
                        }
                    }
                    else if (item.Quantity > 0)
                    {
                        await _cartItemRepository.AddAsync(new CartItem
                        {
                            CartId = cart.CartId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        });
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return await BuildCartResponse(cart.CartId);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Cart?> GetCarts(Guid userId)
        {
            return await _repository.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> PlaceOrderAllFromCart(Guid cartId, Guid userId, Guid addressId, string PaymentType)
        {
            if (cartId == Guid.Empty || userId == Guid.Empty || addressId == Guid.Empty)
                return false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _repository.GetQueryable()
                    .Include(c => c.CartItems!)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return false;

                var addressExists = await _addressRepository.GetQueryable()
                    .AnyAsync(a => a.AddressId == addressId && a.UserId == userId);

                if (!addressExists)
                    return false;

                var cartItems = cart.CartItems.ToList();
                var productIds = cartItems.Select(ci => ci.ProductId).ToList();

                var stocks = await _stockRepository.GetQueryable()
                    .Where(s => productIds.Contains(s.ProductId))
                    .ToListAsync();

                foreach (var item in cartItems)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);

                    if (stock == null)
                        throw new AppException($"Stock not found for {item.Product!.Name}");

                    if (stock.Quantity < item.Quantity)
                        throw new AppException($"Insufficient stock for {item.Product!.Name}");
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

                    stock.Quantity -= item.Quantity;

                    await _stockRepository.UpdateAsync(stock.StockId, stock);

                    order.OrderDetails!.Add(new OrderDetails
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product!.Name,
                        ProductPrice = item.Product.Price,
                        Quantity = item.Quantity
                    });
                }

                await _orderRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                var payment = new Payment
                {
                    UserId = userId,
                    OrderId = order.OrderId,
                    TotalAmount = order.TotalAmount,
                    PaymentType = PaymentType
                };

                await _paymentRepository.AddAsync(payment);

                foreach (var item in cartItems)
                {
                    await _cartItemRepository.DeleteAsync(item.CartItemId);
                }

                await _repository.DeleteAsync(cart.CartId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> RemoveAllFromCartByUserID(Guid userId)
        {
            var cart = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return false;

            var cartItems = await _cartItemRepository.GetQueryable()
                .Where(ci => ci.CartId == cart.CartId)
                .ToListAsync();

            foreach (var item in cartItems)
            {
                await _cartItemRepository.DeleteAsync(item.CartItemId);
            }

            await _repository.DeleteAsync(cart.CartId);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromCart(Guid userId, Guid cartId, Guid productId)
        {
            var cart = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);

            if (cart == null)
                return false;

            var cartItem = await _cartItemRepository.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);

            if (cartItem == null)
                return false;

            await _cartItemRepository.DeleteAsync(cartItem.CartItemId);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task<GetCartResponseDTO> BuildCartResponse(Guid cartId)
        {
            return await _repository.GetQueryable()
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