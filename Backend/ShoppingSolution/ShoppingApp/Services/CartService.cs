using Azure;
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

        public async Task<ApiResponse<GetCartResponseDTO>> GetUserCarts(Guid userId, int pageNumber, int pageSize)
        {
            var userCart = await _repository
                .FirstOrDefaultAsync(c => c.UserId == userId);

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
        public async Task<ApiResponse<AddToCartResponseDTO>> AddCart(Guid UserId,AddToCartRequestDTO request)
        {
            if(request.Quantity <= 0)
            {
                return new ApiResponse<AddToCartResponseDTO>()
                {
                    StatusCode = 400,
                    Message = "Quantity must be greater than zero",
                    Action = "",
                    Data = null
                };
            }

            var product = await _productRepository.GetAsync(request.ProductId);
            if(product == null)
            {
                return new ApiResponse<AddToCartResponseDTO>()
                {
                    StatusCode = 400,
                    Message = "Product not found",
                    Action = "",
                    Data = null
                };
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

                if(existingItem != null)
                {
                    if(request.Quantity > existingItem.Quantity)
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
                    Action = ""
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ApiResponse<OrderAllFromCartResponseDTO>> PlaceOrderAllFromCart(Guid userId, Guid addressId, string PaymentType)
        {

            var userCart = await _repository.GetQueryable().FirstOrDefaultAsync(c => c.UserId == userId);
            if(userCart == null)
            {
                return new ApiResponse<OrderAllFromCartResponseDTO>()
                {
                    StatusCode = 404,
                    Message = "No cart found for this user",
                    Data = null,
                    Action = ""
                };
            }

            var addressExists = await _addressRepository.GetQueryable().FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);

            if (addressExists == null)
            {
                return new ApiResponse<OrderAllFromCartResponseDTO>()
                {
                    StatusCode = 404,
                    Message = "Address not found",
                    Data = null,
                    Action = ""
                };
            }
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _repository.GetQueryable().Include(c => c.CartItems!).ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CartId == userCart.CartId && c.UserId == userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return new ApiResponse<OrderAllFromCartResponseDTO>()
                    {
                        StatusCode = 404,
                        Message = "No cart items found",
                        Data = null,
                        Action = ""
                    }; ;

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

                OrderAllFromCartResponseDTO response = new OrderAllFromCartResponseDTO()
                {
                    IsSuccess = true
                };

                return new ApiResponse<OrderAllFromCartResponseDTO>()
                {
                    StatusCode = 404,
                    Message = "Order placed",
                    Data = response,
                    Action = ""
                }; ;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                return new ApiResponse<OrderAllFromCartResponseDTO>()
                {
                    StatusCode = 404,
                    Message = "Try again",
                    Data = null,
                    Action = ""
                }; ; ;
            }
        }

        public async Task<ApiResponse<RemoveAllFromCartResponseDTO>> RemoveAllFromCartByUserID(Guid userId)
        {
            var cart = await _repository.GetQueryable().FirstOrDefaultAsync(c => c.UserId == userId);

            RemoveAllFromCartResponseDTO response = new RemoveAllFromCartResponseDTO()
            {
                IsRemoved = false,
            };

            if (cart == null)
            {
                return new ApiResponse<RemoveAllFromCartResponseDTO>()
                {
                    StatusCode = 404,
                    Message = "Cart not found for this user",
                    Data = response,
                    Action = ""
                };
            }
                
            var cartItems = await _cartItemRepository.GetQueryable().Where(ci => ci.CartId == cart.CartId).ToListAsync();

            if(!cartItems.Any())
            {
                return new ApiResponse<RemoveAllFromCartResponseDTO>()
                {
                    StatusCode = 404,
                    Message = "No cart items found",
                    Data = response,
                    Action = ""
                };
            }

            foreach (var item in cartItems)
            {
                await _cartItemRepository.DeleteAsync(item.CartItemId);
            }

            await _repository.DeleteAsync(cart.CartId);

            await _unitOfWork.SaveChangesAsync();
            response.IsRemoved = true;

            return new ApiResponse<RemoveAllFromCartResponseDTO>()
            {
                StatusCode = 200,
                Message = "Cart cleared",
                Data = response,
                Action = ""
            };
        }

        public async Task<ApiResponse<RemoveFromCartResponseDTO>> RemoveFromCart(Guid userId, Guid cartId,Guid cartItemId, Guid productId)
        {
            var cart = await _repository.GetQueryable().FirstOrDefaultAsync(c => c.CartId == cartId && c.UserId == userId);
            RemoveFromCartResponseDTO response = new RemoveFromCartResponseDTO()
            {
                IsRemoved = false,
            };

            if (cart == null)
            {
                return new ApiResponse<RemoveFromCartResponseDTO>()
                {
                    StatusCode = 404,
                    Message = "Cart not found",
                    Data = response,
                    Action = ""
                };
            }

            var cartItem = await _cartItemRepository.GetQueryable().FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId && ci.CartItemId == cartItemId);

            if (cartItem == null)
            {
                return new ApiResponse<RemoveFromCartResponseDTO>()
                {
                    StatusCode = 404,
                    Message = "Product not found in this cart",
                    Data = response,
                    Action = ""
                };
            }

            await _cartItemRepository.DeleteAsync(cartItem.CartItemId);
            await _repository.DeleteAsync(cartId);
            response.IsRemoved = true;
            return new ApiResponse<RemoveFromCartResponseDTO>()
            {
                StatusCode = 200,
                Message = "Products removed",
                Data = response,
                Action = ""
            };
        }
    }
}