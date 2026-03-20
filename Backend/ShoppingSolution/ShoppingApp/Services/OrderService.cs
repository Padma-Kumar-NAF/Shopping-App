using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Guid, Stock> _stockRepository;
        private readonly IRepository<Guid, Order> _repository;
        private readonly IRepository<Guid, Payment> _paymentRepository;
        private readonly IRepository<Guid, Refund> _refundRepository;
        private readonly IRepository<Guid, User> _userRepository;
        private readonly IRepository<Guid, Address> _addressRepository;
        private readonly IRepository<Guid,Product> _productRepository;
        private readonly IRepository<Guid, OrderDetails> _orderDetailsRepository;

        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            IRepository<Guid, Order> repository,
            IRepository<Guid, Stock> stockRepository,
            IRepository<Guid, Payment> paymentRepository,
            IRepository<Guid, Refund> refundRepository,
            IRepository<Guid, User> userRepository,
            IRepository<Guid, Address> addressRepository,
            IRepository<Guid, Product> productRepository,
            IRepository<Guid, OrderDetails> orderDetailsRepository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _stockRepository = stockRepository;
            _paymentRepository = paymentRepository;
            _refundRepository = refundRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _addressRepository = addressRepository;
            _productRepository = productRepository;
            _orderDetailsRepository = orderDetailsRepository;
        }

        public async Task<ApiResponse<CancelOrderResponseDTO>> CancelOrder(Guid userId, Guid orderId)
        {
            
            if(await IsUserNotFound(userId))
            {
                throw new AppException("User not found",404);
            }

            await _unitOfWork.BeginTransactionAsync();

            var order = await _repository.GetQueryable().Include(o => o.OrderDetails!).Include(o => o.Address).Include(o => o.Payment)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                throw new AppException("Order not found.",404);
            }

            if (order.Status == "Cancelled")
            {
                throw new AppException("Order already cancelled.",400);
            }

            if (order.Status == "Delivered")
            {
                throw new AppException("Cannot cancel delivered order.",400);
            }

            try
            {
                foreach (var item in order.OrderDetails!)
                {
                    var stock = await _stockRepository.GetQueryable().FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                    if (stock == null)
                    {
                        // Dont throw error , its a admin side problem , so user can cancel the order
                        //throw new AppException($"Stock not found for product {item.ProductId}");
                        continue;
                    }

                    stock.Quantity += item.Quantity;

                    await _stockRepository.UpdateAsync(stock.StockId, stock);
                }

                order.Status = "Cancelled";
                await _repository.UpdateAsync(order.OrderId, order);

                var refund = new Refund
                {
                    UserId = userId,
                    OrderId = order.OrderId,
                    PaymentId = order.Payment!.PaymentId,
                    RefundAmount = order.TotalAmount
                };

                await _refundRepository.AddAsync(refund);

                await _unitOfWork.CommitAsync();

                return new ApiResponse<CancelOrderResponseDTO>()
                {
                    StatusCode = 200,
                    Data = new CancelOrderResponseDTO()
                    {
                        IsSuccess = true,
                        RefuncdAmount = order.TotalAmount
                    },
                    Message = "Order cancel successfully",
                    Action = "DeleteOrder"
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
                throw new AppException("Error while canceling order", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while canceling order", ex, 500);
            }
        }

        public async Task<ApiResponse<GetUserOrderDetailsResponseDTO>> GetUserOrderById(Guid userId,GetUserOrderDetailsRequestDTO request)
        {
            try
            {

                if (await IsUserNotFound(userId))
                {
                    throw new AppException("User not found", 404);
                }
                var query = _repository.GetQueryable().Where(order => order.UserId == userId).OrderByDescending(o => o.CreatedAt);

                var orders = await query
                .Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
                .Take(request.pagination.PageSize)
                .Select(o => new OrderDetailsResponseDTO
                {
                    OrderId = o.OrderId,
                    Status = o.Status,
                    TotalProductsCount = o.TotalProductsCount,
                    TotalAmount = o.TotalAmount,
                    DeliveryDate = (DateTime)o.DeliveryDate!,

                    Address = new AddressDTO
                    {
                        AddressId = o.Address!.AddressId,
                        AddressLine1 = o.Address.AddressLine1,
                        AddressLine2 = o.Address.AddressLine2,
                        State = o.Address.State,
                        City = o.Address.City,
                        Pincode = o.Address.Pincode
                    },

                    Payment = new PaymentDTO
                    {
                        PaymentId = o.Payment!.PaymentId,
                        PaymentType = o.Payment.PaymentType
                    },

                    Items = o.OrderDetails!
                    .Select(od => new OrderDetailsDTO
                    {
                        OrderDetailsId = od.OrderDetailsId,
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        ProductPrice = od.ProductPrice,
                        ImagePath = od.Product!.ImagePath
                    })
                    .OrderBy(od => od.ProductName)
                    .ToList()
                })
                .ToListAsync();

                return new ApiResponse<GetUserOrderDetailsResponseDTO>
                {
                    StatusCode = 200,
                    Message = orders.Any() ? "Orders fetched successfully" : "No orders found",
                    Data = new GetUserOrderDetailsResponseDTO
                    {
                        Items = orders
                    },
                    Action = "GetUserOrders"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while Fetching users orders", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while Fetching users orders", ex, 500);
            }
        }

        public async Task<ApiResponse<PlaceOrderResponseDTO>> PlaceOrder(Guid userId,PlaceOrderRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (await IsUserNotFound(userId))
                {
                    throw new AppException("User not found", 404);
                }

                var Address = await _addressRepository.GetAsync(request.AddressId);

                if (Address == null)
                {
                    throw new AppException($"{nameof(Address)} not found", 404);    
                }

                var Product = await _productRepository.GetAsync(request.OrderProductdDetails.ProductId);

                if (Product == null)
                {
                    throw new AppException("Product not found", 404);
                }
                var stock = await _stockRepository.FirstOrDefaultAsync(s => s.ProductId == request.OrderProductdDetails.ProductId);

                if (stock == null)
                    throw new AppException($"Stock not found for product {request.OrderProductdDetails.ProductId}",404);

                if (stock.Quantity < request.OrderProductdDetails.Quantity)
                    throw new AppException($"Insufficient stock for product {request.OrderProductdDetails.ProductName}",404);

                var order = new Order
                {
                    UserId = userId,
                    Status = "Not Delivered",
                    TotalAmount = request.TotalAmount,
                    TotalProductsCount = request.TotalProductsCount,
                    AddressId = request.AddressId,
                    DeliveryDate = DateTime.Now.AddDays(2),
                };

                stock.Quantity -= request.OrderProductdDetails.Quantity;

                var orderDetails = new OrderDetails()
                {
                    OrderId = order.OrderId,
                    ProductId = request.OrderProductdDetails.ProductId,
                    ProductName = request.OrderProductdDetails.ProductName,
                    ProductPrice = request.OrderProductdDetails.ProductPrice,
                    Quantity = request.OrderProductdDetails.Quantity
                };

                var payment = new Payment
                {
                    UserId = userId,
                    OrderId = order.OrderId,
                    TotalAmount = request.TotalAmount,
                    PaymentType = request.PaymentType
                };

                await _repository.AddAsync(order);

                await _orderDetailsRepository.AddAsync(orderDetails);

                await _stockRepository.UpdateAsync(stock.StockId, stock);

                await _paymentRepository.AddAsync(payment);

                await _unitOfWork.CommitAsync();

                PlaceOrderResponseDTO response = new PlaceOrderResponseDTO()
                {
                    IsSuccess = true,
                    OrderId = order.OrderId,
                    PaymentId = payment.PaymentId,
                    DeliveryDate = (DateTime)order.DeliveryDate,
                    OrderDetailsId = orderDetails.OrderDetailsId,
                };

                return new ApiResponse<PlaceOrderResponseDTO>()
                {
                    StatusCode = 200,
                    Data = response,
                    Action = "OrderedConfirmed",
                    Message = "Ordered Successfully"
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
                throw new AppException("Error while Placing the order", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while placing the user order", ex, 500);
            }
        }

        public async Task<ApiResponse<UpdateOrderResponseDTO>> UpdateOrder(Guid userId,Guid OrderId, string Status)
        {
            if(await IsUserNotFound(userId))
            {
                throw new AppException("User not found",401);
            }

            if(Status != "Cancel" || Status != "Delivered" || Status != "Shipped" || Status != "Not Delivered")
            {
                throw new AppException("Invalid Status",400);
            }
            try
            {
                var Order = await _repository.GetAsync(OrderId);
                if (Order == null)
                {
                    throw new AppException($"{nameof(Order)} is null.");
                }

                Order.Status = Status;

                await _repository.UpdateAsync(Order.OrderId, Order);

                return new ApiResponse<UpdateOrderResponseDTO>()
                {
                    StatusCode = 200,
                    Data = new UpdateOrderResponseDTO()
                    {
                        IsUpdated = true,
                    },
                    Message = "Order updated successfully",
                    Action = "Update order status"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while updating order", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while updating the order", ex, 500);
            }
        }

        private async Task<bool > IsUserNotFound(Guid userId)
        {
            var user = await _userRepository.GetAsync(userId);
            return user == null;
        }
    }
}