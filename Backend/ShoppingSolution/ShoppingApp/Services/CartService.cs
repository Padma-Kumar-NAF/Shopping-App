using Microsoft.EntityFrameworkCore;
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
        private readonly IRepository<Guid, PromoCode> _promoRepository;
        private readonly IRepository<Guid, Wallet> _walletRepository;

        private readonly IUnitOfWork _unitOfWork;

        public CartService(
            IRepository<Guid, Cart> repository,
            IRepository<Guid, CartItem> cartItemRepository,
            IRepository<Guid, Product> productRepository,
            IRepository<Guid, Stock> stockRepository,
            IRepository<Guid, Order> orderRepository,
            IRepository<Guid, Payment> paymentRepository,
            IRepository<Guid, Address> addressRepository,
            IRepository<Guid, PromoCode> promoRepository,
            IRepository<Guid, Wallet> walletRepository,
            IUnitOfWork unitOfWork)
            {
                _repository = repository;
                _cartItemRepository = cartItemRepository;
                _productRepository = productRepository;
                _stockRepository = stockRepository;
                _orderRepository = orderRepository;
                _paymentRepository = paymentRepository;
                _addressRepository = addressRepository;
                _promoRepository = promoRepository;
                _walletRepository = walletRepository;
                _unitOfWork = unitOfWork;
            }

        public async Task<ApiResponse<AddToCartResponseDTO>> AddCart(Guid UserId, AddToCartRequestDTO request)
        {
            if (request.Quantity <= 0)
            {
                throw new AppException("Quantity must be greater than zero",400);
            }

            var product = await _productRepository.GetAsync(request.ProductId);
            if (product == null)
            {
                throw new AppException("Product not found",404);
            }
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _repository.GetQueryable().FirstOrDefaultAsync(c => c.UserId == UserId);

                if (cart == null)
                {
                    cart = new Cart { UserId = UserId };
                    await _repository.AddAsync(cart);
                }

                CartItem cartItem;

                var existingItem = await _cartItemRepository.FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == request.ProductId);

                if (existingItem != null)
                {
                    if (request.Quantity > existingItem.Quantity)
                    {
                        existingItem.Quantity = request.Quantity;
                    }
                    cartItem = existingItem;
                }
                else
                {
                    CartItem newCartItem = new CartItem()
                    {
                        CartId = cart.CartId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                    };

                    cartItem = newCartItem;

                    await _cartItemRepository.AddAsync(newCartItem);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var response = new AddToCartResponseDTO()
                {
                    CartId = cart.CartId,
                    CartItemId = cartItem.CartItemId
                };

                return new ApiResponse<AddToCartResponseDTO>()
                {
                    StatusCode = 200,
                    Message = "Product added Successfully",
                    Data = response,
                    Action = "Add product to cart"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Database error while adding to cart", ex,500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while adding to cart", ex,500);
            }
        }

        public async Task<ApiResponse<GetCartResponseDTO>> GetUserCarts(Guid userId, int pageNumber, int pageSize)
        {
            try
            {
                var userCart = await _repository.FirstOrDefaultAsync(c => c.UserId == userId);

                if (userCart == null)
                {
                    return new ApiResponse<GetCartResponseDTO>
                    {
                        StatusCode = 200,
                        Message = "Cart not found",
                        Action = "",
                        Data = new GetCartResponseDTO()
                    };
                }

                var pagedItems = _cartItemRepository
                    .GetQueryable()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Where(ci => ci.CartId == userCart.CartId)
                    .Select(ci => new CartItemDTO
                    {
                        CartItemId = ci.CartItemId,
                        ProductId = ci.ProductId,
                        CategoryId = ci.Product!.CategoryId,
                        ProductName = ci.Product.Name,
                        ImagePath = ci.Product.ImagePath,
                        Description = ci.Product.Description,
                        Price = ci.Product.Price,
                        Quantity = ci.Quantity
                    }).ToList();

                var response = new GetCartResponseDTO
                {
                    CartId = userCart.CartId,
                    CartItems = pagedItems,
                };

                return new ApiResponse<GetCartResponseDTO>
                {
                    StatusCode = 200,
                    Message = pagedItems == null
                        ? "Cart is empty"
                        : "Cart fetched successfully",
                    Action = "",
                    Data = response
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Database error while getting the user cart", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while getting the user cart", ex, 500);
            }
        }

        public async Task<ApiResponse<OrderAllFromCartResponseDTO>> PlaceOrderAllFromCart(Guid userId,Guid addressId,string paymentType,string promoCode,bool useWallet,string stripePaymentId = "")
        {
            var userCart = await _repository.GetQueryable().FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                throw new AppException("No cart found for this user", 404);
            }

            var addressExists = await _addressRepository.GetQueryable().FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);

            if (addressExists == null)
            {
                throw new AppException("Address not found", 404);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _repository.GetQueryable()
                    .Include(c => c.CartItems!)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CartId == userCart.CartId && c.UserId == userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                {
                    throw new AppException("No cart items found", 404);
                }
                    
                var cartItems = cart.CartItems.ToList();
                var productIds = cartItems.Select(ci => ci.ProductId).ToList();

                var stocks = await _stockRepository.GetQueryable()
                    .Where(s => productIds.Contains(s.ProductId))
                    .ToListAsync();

                foreach (var item in cartItems)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);

                    if (stock == null)
                    {
                        throw new AppException($"Stock not found for {item.Product!.Name}", 404);
                    }

                    if (stock.Quantity < item.Quantity)
                    {
                        throw new AppException($"Insufficient stock for {item.Product!.Name}", 409);
                    }
                }

                decimal subtotal = cartItems.Sum(x => x.Quantity * x.Product!.Price);

                decimal tax = Math.Round(subtotal * 0.18m, 2);
                decimal shipping = subtotal > 5000 ? 0m : 100m;

                decimal orderTotal = subtotal + tax + shipping;

                int discountPercentage = 0;
                decimal discountAmount = 0;
                Guid? promoCodeId = null;

                if (!string.IsNullOrWhiteSpace(promoCode))
                {
                    var promo = await _promoRepository.GetQueryable()
                        .FirstOrDefaultAsync(p =>
                            p.PromoCodeName == promoCode.Trim().ToUpper() &&
                            !p.IsDeleted);

                    if (promo == null)
                        throw new AppException("Invalid promo code", 400);

                    var now = DateTime.UtcNow.Date;

                    if (now < promo.FromDate.Date)
                        throw new AppException("Promo code is not active yet", 400);

                    if (now > promo.ToDate.Date)
                        throw new AppException("Promo code has expired", 400);

                    discountPercentage = promo.DiscountPercentage;
                    discountAmount = Math.Round(subtotal * discountPercentage / 100, 2);
                    promoCodeId = promo.PromoCodeId;
                }

                decimal amountAfterDiscount = orderTotal - discountAmount;

                // ── Wallet (SAME as PlaceOrder) ──────────────────
                decimal walletUsed = 0;

                if (useWallet)
                {
                    walletUsed = await HandleWalletPayment(userId,amountAfterDiscount);
                }

                decimal finalAmount = amountAfterDiscount - walletUsed;

                string paymentStatus =
                    walletUsed >= amountAfterDiscount ? "Full Wallet" :
                    walletUsed > 0 ? "Partial Wallet" :
                    "External Payment";

                // ── Create Order ────────────────────────────────
                var order = new Order
                {
                    UserId = userId,
                    Status = "Not Delivered",
                    TotalProductsCount = cartItems.Sum(x => x.Quantity),

                    // Before discount
                    TotalAmount = orderTotal,

                    // After discount
                    OrderTotalAmount = amountAfterDiscount,

                    DiscountPercentage = discountPercentage,
                    DiscountAmount = discountAmount,
                    AddressId = addressId,
                    DeliveryDate = DateTime.UtcNow.AddDays(2),
                    PromoCodeId = promoCodeId,
                    OrderDetails = new List<OrderDetails>()
                };

                // ── Order Details + Stock Update ────────────────
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

                // ── Payment ─────────────────────────────────────
                var payment = new Payment
                {
                    UserId = userId,
                    OrderId = order.OrderId,
                    TotalAmount = amountAfterDiscount,
                    PaymentType = paymentType,
                    StripePaymentId = stripePaymentId
                };

                await _paymentRepository.AddAsync(payment);

                // ── Clear Cart ─────────────────────────────────
                foreach (var item in cartItems)
                    await _cartItemRepository.DeleteAsync(item.CartItemId);

                await _repository.DeleteAsync(cart.CartId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new ApiResponse<OrderAllFromCartResponseDTO>
                {
                    StatusCode = 200,
                    Message = "Order placed successfully",
                    Data = new OrderAllFromCartResponseDTO
                    {
                        IsSuccess = true,
                        OrderId = order.OrderId,
                        Subtotal = subtotal,
                        Tax = tax,
                        Shipping = shipping,
                        DiscountPercentage = discountPercentage,
                        DiscountAmount = discountAmount,
                        WalletUsed = walletUsed,
                        FinalAmount = finalAmount,
                        PaymentStatus = paymentStatus
                    },
                    Action = "PlaceOrderAllFromCart"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Database error while placing order", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while placing order", ex, 500);
            }
        }

        private async Task<decimal> HandleWalletPayment(Guid userId, decimal finalAmount)
        {
            var wallet = await _walletRepository.GetQueryable().FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                throw new AppException("Wallet not found", 404);
            }

            decimal walletBalance = wallet.WalletAmount;

            decimal walletUsed;

            if (walletBalance >= finalAmount)
            {
                walletUsed = finalAmount;
                wallet.WalletAmount -= finalAmount;
            }
            else
            {
                walletUsed = walletBalance;
                wallet.WalletAmount = 0;
            }

            await _walletRepository.UpdateAsync(wallet.WalletId, wallet);

            return walletUsed;
        }

        public async Task<ApiResponse<RemoveAllFromCartResponseDTO>> RemoveAllFromCartByUserID(Guid userId)
        {
            var cart = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                throw new AppException("Cart not found for this user", 404);
            }

            var cartItems = await _cartItemRepository.GetQueryable()
                .Where(ci => ci.CartId == cart.CartId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                throw new AppException("No cart items found", 404);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var item in cartItems)
                {
                    await _cartItemRepository.DeleteAsync(item.CartItemId);
                }

                await _repository.DeleteAsync(cart.CartId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new ApiResponse<RemoveAllFromCartResponseDTO>()
                {
                    StatusCode = 200,
                    Message = "Cart cleared successfully",
                    Data = new RemoveAllFromCartResponseDTO
                    {
                        IsRemoved = true
                    },
                    Action = "RemoveAllFromCart"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Database error while clearing cart", ex,500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while clearing cart", ex,500);
            }
        }

        public async Task<ApiResponse<RemoveFromCartResponseDTO>> RemoveFromCart(Guid userId, Guid cartId, Guid cartItemId, Guid productId)
        {
            var cart = await _repository.GetQueryable().FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);

            var cartItem = await _cartItemRepository.GetQueryable().FirstOrDefaultAsync(ci =>ci.CartId == cartId && ci.CartItemId == cartItemId);

            if (cartItem == null)
            {
                throw new AppException("Cart item not found in this cart", 404);
            }

            if (cartItem.ProductId != productId)
            {
                throw new AppException("Product mismatch for this cart item", 400);
            }

            try
            {
                await _cartItemRepository.DeleteAsync(cartItem.CartItemId);
                return new ApiResponse<RemoveFromCartResponseDTO>()
                {
                    StatusCode = 200,
                    Message = "Product removed from cart",
                    Data = new RemoveFromCartResponseDTO
                    {
                        IsRemoved = true
                    },
                    Action = "RemoveFromCart"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Database error while removing item from cart", ex,500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while removing item from cart", ex, 500);
            }
        }

        public async Task<ApiResponse<UpdateUserCartResponseDTO>> UpdateCart(Guid userId,Guid cartId,Guid cartItemId,Guid productId,int quantity)
        {
            var cart = await _repository.GetQueryable()
                .FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);

            if (cart == null)
            {
                throw new AppException("Cart not found", 404);
            }

            var cartItem = await _cartItemRepository.GetQueryable()
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cartId &&
                    ci.CartItemId == cartItemId);

            if (cartItem == null)
            {
                throw new AppException("Cart item not found in this cart", 404);
            }

            if (cartItem.ProductId != productId)
            {
                throw new AppException("Product mismatch for this cart item", 400);
            }

            if (quantity <= 0)
            {
                throw new AppException("Quantity must be greater than zero", 400);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                cartItem.Quantity = quantity;

                await _cartItemRepository.UpdateAsync(cartItemId,cartItem);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new ApiResponse<UpdateUserCartResponseDTO>()
                {
                    StatusCode = 200,
                    Message = "Cart updated successfully",
                    Data = new UpdateUserCartResponseDTO
                    {
                        IsUpdated = true
                    },
                    Action = "UpdateCart"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Database error while updating cart", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while updating cart", ex, 500);
            }
        }
    }
}