using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Order;
using ShoppingApp.Models.DTOs.Promocode;

namespace ShoppingApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Guid, Order> _repository;
        private readonly IRepository<Guid, Stock> _stockRepository;
        private readonly IRepository<Guid, Payment> _paymentRepository;
        private readonly IRepository<Guid, Refund> _refundRepository;
        private readonly IRepository<Guid, User> _userRepository;
        private readonly IRepository<Guid, Address> _addressRepository;
        private readonly IRepository<Guid,Product> _productRepository;
        private readonly IRepository<Guid, OrderDetails> _orderDetailsRepository;
        private readonly IRepository<Guid, PromoCode> _promoRepository;
        private readonly IRepository<Guid, Wallet> _walletRepository;

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
            IRepository<Guid, PromoCode> promoRepository,
            IRepository<Guid, Wallet> walletRepository,
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
            _promoRepository = promoRepository;
            _walletRepository = walletRepository;
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

            // Indhu work
            if (order.DeliveryDate == DateTime.MinValue)
            {
                throw new AppException("You can't cancel the order --------- ",401);
            }

            if (order.Status == "Cancelled")
            {
                throw new AppException("Order already cancelled.",400);
            }

            //if (order.Status == "Delivered")
            //{
            //    throw new AppException("Cannot cancel delivered order.",400);
            //}

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

        public async Task<ApiResponse<GetAllOrderResponseDTO>> GetAllOrders(GetAllOrderRequestDTO request)
        {
            try
            {
                var query = _repository.GetQueryable()
                            .Include(o => o.User)
                            .Include(o => o.Address)
                            .Include(o => o.Payment)
                            .Include(o => o.Refund)
                            .Include(o => o.OrderDetails)
                             .ThenInclude(od => od.Product);

                var totalOrders = await query.CountAsync();

                if (totalOrders == 0)
                {
                    return new ApiResponse<GetAllOrderResponseDTO>
                    {
                        StatusCode = 404,
                        Message = "No orders found",
                        Data = new GetAllOrderResponseDTO(),
                        Action = "GetAllOrders"
                    };
                }

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
                    .Take(request.pagination.PageSize)
                    .ToListAsync();

                var response = new GetAllOrderResponseDTO
                {
                    Items = orders.Select(o => new OrderDetailsResponseDTO
                    {
                        OrderId = o.OrderId,
                        Status = o.Status,
                        DeliveryDate = o.DeliveryDate!,

                        TotalProductsCount = o.OrderDetails!.Count,
                        TotalAmount = o.OrderTotalAmount,
                        IsRefunded = o.Refund?.RefundId != null ? true : false,

                        OrderBy = new OrderBy
                        {
                            UserEmail = o.User!.Email,
                            UserName = o.User.Name,
                        },

                        Address = new AddressDTO
                        {
                            AddressId = o.Address!.AddressId,
                            AddressLine1 = o.Address.AddressLine1,
                            AddressLine2 = o.Address.AddressLine2,
                            City = o.Address.City,
                            State = o.Address.State,
                            Pincode = o.Address.Pincode
                        },

                        Payment = new PaymentDTO
                        {
                            PaymentId = o.Payment!.PaymentId,
                            PaymentType = o.Payment.PaymentType
                        },

                        Items = o.OrderDetails.Select(od => new OrderDetailsDTO
                        {
                            OrderDetailsId = od.OrderDetailsId,
                            ProductId = od.ProductId,
                            ProductName = od.Product!.Name,
                            ImagePath = od.Product.ImagePath,
                            Quantity = od.Quantity,
                            ProductPrice = od.ProductPrice
                        }).ToList()

                    }).ToList()
                };

                return new ApiResponse<GetAllOrderResponseDTO>
                {
                    StatusCode = 200,
                    Message = "Orders fetched successfully",
                    Data = response,
                    Action = "GetAllOrders"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while Fetching orders", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while Fetching orders", ex, 500);
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
                        TotalAmount = o.OrderTotalAmount,
                        DeliveryDate = (DateTime)o.DeliveryDate!,

                        IsRefunded = _refundRepository.GetQueryable().Where(r => r.OrderId == o.OrderId).FirstOrDefault() != null ? true : false,

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

        public async Task<ApiResponse<OrderRefundResponseDTO>> OrderRefund(Guid userId, OrderRefundRequestDTO request)
        {
            //Console.WriteLine("---------------");
            //Console.WriteLine(userId);
            //Console.WriteLine("Total amount"+request.TotalAmount);
            //Console.WriteLine("Payment id"+request.PaymentId);
            //Console.WriteLine("Order id"+request.OrderId);
            //Console.WriteLine("---------------");

            var user = await _userRepository.GetAsync(userId);
            if(user == null || user.Role != "admin")
            {
                throw new AppException("You can't make refund , access denied");
            }
            if (request.TotalAmount <= 0)
            {
                throw new AppException("Total amount must be greater than 0");
            }

            if (await IsUserNotFound(userId))
            {
                throw new AppException("User not found", 404);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _repository.GetAsync(request.OrderId);
                if(order == null)
                {
                    throw new AppException("Order not found", 404);
                }

                var payment = await _paymentRepository.GetAsync(request.PaymentId);
                if(payment == null)
                {
                    throw new AppException("Payment not found", 404);
                }

                var existingRefund = await _refundRepository.GetQueryable().FirstOrDefaultAsync(r => r.OrderId == request.OrderId);
                if (existingRefund != null)
                {
                    throw new AppException("Refund already processed", 400);
                }

                var refund = new Refund
                {
                    UserId = order.UserId,
                    OrderId = request.OrderId,
                    PaymentId = request.PaymentId,
                    RefundAmount = request.TotalAmount
                };

                await _refundRepository.AddAsync(refund);

                var wallet = await _walletRepository.GetQueryable().FirstOrDefaultAsync(w => w.UserId == order.UserId);

                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        UserId = order.UserId,
                        WalletAmount = request.TotalAmount
                    };

                    await _walletRepository.AddAsync(wallet);
                }
                else
                {
                    wallet.WalletAmount += request.TotalAmount;
                    await _walletRepository.UpdateAsync(wallet.WalletId, wallet);
                }

                await _unitOfWork.CommitAsync();

                return new ApiResponse<OrderRefundResponseDTO>()
                {
                    StatusCode = 200,
                    Data = new OrderRefundResponseDTO()
                    {
                        IsRefund = true
                    },
                    Action = "UpdateRefund",
                    Message = "Refund successful and wallet credited"
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
                throw new AppException("Error while refunding the amount", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while refunding", ex, 500);
            }
        }
        public async Task<ApiResponse<PlaceOrderResponseDTO>> PlaceOrder(Guid userId, PlaceOrderRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (await IsUserNotFound(userId))
                    throw new AppException("User not found", 404);

                var address = await _addressRepository.GetAsync(request.AddressId)
                    ?? throw new AppException("Address not found", 404);

                var product = await _productRepository.GetAsync(request.OrderProductdDetails.ProductId)
                    ?? throw new AppException("Product not found", 404);

                var stock = await _stockRepository.FirstOrDefaultAsync(s => s.ProductId == product.ProductId)
                    ?? throw new AppException("Stock not found", 404);

                if (stock.Quantity < request.OrderProductdDetails.Quantity)
                    throw new AppException("Insufficient stock", 400);

                decimal tax      = Math.Round(request.TotalAmount * 0.18m, 2);
                decimal shipping = request.TotalAmount > 5000 ? 0m : 99m;
                decimal orderTotal = request.TotalAmount + tax + shipping;

                int discountPercentage = 0;
                decimal discountAmount = 0;
                Guid? promoCodeId = null;

                if (!string.IsNullOrWhiteSpace(request.PromoCode))
                {
                    var promo = await _promoRepository.GetQueryable()
                        .FirstOrDefaultAsync(p => p.PromoCodeName == request.PromoCode.Trim().ToUpper() && !p.IsDeleted);

                    if (promo == null)
                        throw new AppException("Invalid promo code", 400);

                    var now = DateTime.UtcNow.Date;
                    if (now < promo.FromDate.Date)
                        throw new AppException("Promo code is not active yet", 400);
                    if (now > promo.ToDate.Date)
                        throw new AppException("Promo code has expired", 400);

                    discountPercentage = promo.DiscountPercentage;
                    discountAmount = Math.Round(orderTotal * discountPercentage / 100, 2);
                    promoCodeId = promo.PromoCodeId;
                }

                var amountAfterDiscount = orderTotal - discountAmount;

                decimal walletUsed = 0;

                if (request.UseWallet)
                {
                    walletUsed = await HandleWalletPayment(userId, amountAfterDiscount);
                }

                var finalAmount = amountAfterDiscount - walletUsed;

                var order = CreateOrder(userId, request, discountPercentage, discountAmount, amountAfterDiscount, promoCodeId);
                await _repository.AddAsync(order);

                var orderDetails = await CreateOrderDetailsAndUpdateStock(request, stock, order);

                var payment = await CreatePayment(userId, order, finalAmount, request.PaymentType);

                await _unitOfWork.CommitAsync();

                return new ApiResponse<PlaceOrderResponseDTO>
                {
                    StatusCode = 200,
                    Data = new PlaceOrderResponseDTO
                    {
                        IsSuccess = true,
                        OrderId = order.OrderId,
                        PaymentId = payment.PaymentId,
                        DeliveryDate = (DateTime)order.DeliveryDate!,
                        OrderDetailsId = orderDetails.OrderDetailsId,
                        Subtotal = request.TotalAmount,
                        Tax = tax,
                        Shipping = shipping,
                        DiscountPercentage = discountPercentage,
                        DiscountAmount = discountAmount,
                        WalletUsed = walletUsed,
                        FinalAmount = finalAmount,
                    },
                    Message = "Order placed successfully",
                    Action = "OrderedConfirmed"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Error while placing order", ex, 500);
            }
        }

        private Order CreateOrder(Guid userId, PlaceOrderRequestDTO request, int discountPercentage, decimal discountAmount, decimal amountAfterDiscount, Guid? promoCodeId)
        {
            decimal tax      = Math.Round(request.TotalAmount * 0.18m, 2);

            decimal shipping = request.TotalAmount > 5000 ? 0m : 99m;

            decimal orderTotal = request.TotalAmount + tax + shipping;

            return new Order
            {
                UserId = userId,
                Status = "Not Delivered",
                TotalAmount = orderTotal,
                TotalProductsCount = request.TotalProductsCount,
                AddressId = request.AddressId,
                DeliveryDate = DateTime.UtcNow.AddDays(2),
                DiscountPercentage = discountPercentage,
                DiscountAmount = discountAmount,
                OrderTotalAmount = amountAfterDiscount,
                PromoCodeId = promoCodeId
            };
        }

        private async Task<Payment> CreatePayment(Guid userId, Order order, decimal finalAmount, string paymentType)
        {
            var payment = new Payment
            {
                UserId = userId,
                OrderId = order.OrderId,
                TotalAmount = finalAmount,
                PaymentType = paymentType
            };

            await _paymentRepository.AddAsync(payment);
            return payment;
        }

        private async Task<decimal> HandleWalletPayment(Guid userId, decimal finalAmount)
        {
            var wallet = await _walletRepository
                .GetQueryable()
                .FirstOrDefaultAsync(w => w.UserId == userId);

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

        private async Task<OrderDetails> CreateOrderDetailsAndUpdateStock(PlaceOrderRequestDTO request, Stock stock, Order order)
        {
            stock.Quantity -= request.OrderProductdDetails.Quantity;
            await _stockRepository.UpdateAsync(stock.StockId, stock);

            var orderDetails = new OrderDetails
            {
                OrderId = order.OrderId,
                ProductId = request.OrderProductdDetails.ProductId,
                ProductName = request.OrderProductdDetails.ProductName,
                ProductPrice = request.OrderProductdDetails.ProductPrice,
                Quantity = request.OrderProductdDetails.Quantity
            };

            await _orderDetailsRepository.AddAsync(orderDetails);

            return orderDetails;
        }

        public async Task<ApiResponse<UpdateOrderResponseDTO>> UpdateOrder(Guid userId,Guid OrderId, string Status)
        {
            if(await IsUserNotFound(userId))
            {
                throw new AppException("User not found",401);
            }

            if(Status != "Cancelled" && Status != "Delivered" && Status != "Shipped" && Status != "Not Delivered")
            {
                throw new AppException("Invalid Status",400);
            }
            if(Status == "Cancelled")
            {
                throw new AppException("Admin cannot cancel the order", 401);
            }
            try
            {
                var Order = await _repository.GetAsync(OrderId);
                if (Order == null)
                {
                    throw new AppException($"{nameof(Order)} is null.");
                }

                if(Order.Status == "Cancelled" )
                {
                    throw new AppException("Order cancelled");
                }

                if(Order.Status == "Delivered")
                {
                    throw new AppException("Order delivered");
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