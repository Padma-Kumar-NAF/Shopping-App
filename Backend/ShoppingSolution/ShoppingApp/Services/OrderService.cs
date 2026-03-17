using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Guid, Stock> _stockRepository;
        private readonly IRepository<Guid, Order> _repository;
        private readonly IRepository<Guid, Payment> _paymentRepository;
        private readonly IRepository<Guid, Refund> _refundRepository;

        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            IRepository<Guid, Order> repository,
            IRepository<Guid, Stock> stockRepository,
            IRepository<Guid, Payment> paymentRepository,
            IRepository<Guid, Refund> refundRepository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _stockRepository = stockRepository;
            _paymentRepository = paymentRepository;
            _refundRepository = refundRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<GetUserOrderDetailsResponseDTO> CancelOrder(CancelOrderRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _repository.GetQueryable()
                    .Include(o => o.OrderDetails!)
                    .Include(o => o.Address)
                    .Include(o => o.Payment)
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

                if (order == null)
                    throw new Exception("Order not found.");

                if (order.Status == "Cancelled")
                    throw new Exception("Order already cancelled.");

                if (order.Status == "Delivered")
                    throw new Exception("Cannot cancel delivered order.");

                foreach (var item in order.OrderDetails!)
                {
                    var stock = await _stockRepository.GetQueryable()
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                    if (stock == null)
                        throw new Exception($"Stock not found for product {item.ProductId}");

                    stock.Quantity += item.Quantity;

                    await _stockRepository.UpdateAsync(stock.StockId, stock);
                }

                order.Status = "Cancelled";
                await _repository.UpdateAsync(order.OrderId, order);

                var refund = new Refund
                {
                    UserId = request.UserId,
                    OrderId = order.OrderId,
                    PaymentId = order.Payment!.PaymentId,
                    RefundAmount = order.TotalAmount
                };

                await _refundRepository.AddAsync(refund);

                await _unitOfWork.CommitAsync();

                return new GetUserOrderDetailsResponseDTO
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Status = order.Status,
                    TotalProductsCount = order.TotalProductsCount,
                    TotalAmount = order.TotalAmount,
                    DeliveryDate = (DateTime)order.DeliveryDate!,

                    Address = new AddressDTO
                    {
                        AddressId = order.Address!.AddressId,
                        AddressLine1 = order.Address.AddressLine1,
                        AddressLine2 = order.Address.AddressLine2,
                        City = order.Address.City,
                        State = order.Address.State,
                        Pincode = order.Address.Pincode
                    },

                    Payment = new PaymentDTO
                    {
                        PaymentId = order.Payment.PaymentId,
                        PaymentType = order.Payment.PaymentType,
                    },

                    Items = order.OrderDetails.Select(item => new OrderDetailsDTO
                    {
                        OrderDetailsId = item.OrderDetailsId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        ProductPrice = item.ProductPrice
                    }).ToList()
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<GetUserOrderDetailsResponseDTO>> GetUserOrderById(GetUserOrderDetailsRequestDTO request)
        {
            var query = _repository.GetQueryable()
                .Where(order => order.UserId == request.UserId)
                .OrderByDescending(o => o.CreatedAt);

            return await query
                .Skip((request.PageNumber - 1) * request.Limit)
                .Take(request.Limit)
                .Select(o => new GetUserOrderDetailsResponseDTO
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
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

                    Payment = new PaymentDTO()
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
        }

        public async Task<GetUserOrderDetailsResponseDTO> PlaceOrder(PlaceOrderRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var productIds = request.Items.Select(i => i.ProductId).ToList();

                var stocks = await _stockRepository.GetQueryable()
                    .Where(s => productIds.Contains(s.ProductId))
                    .ToListAsync();

                foreach (var item in request.Items)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);

                    if (stock == null)
                        throw new Exception($"Stock not found for product {item.ProductId}");

                    if (stock.Quantity < item.Quantity)
                        throw new Exception($"Insufficient stock for product {item.ProductName}");
                }

                var order = new Order
                {
                    UserId = request.UserId,
                    Status = "Not Delivered",
                    TotalAmount = request.TotalAmount,
                    TotalProductsCount = request.TotalProductsCount,
                    AddressId = request.AddressId,
                    DeliveryDate = DateTime.Now.AddDays(2),
                    OrderDetails = new List<OrderDetails>()
                };

                foreach (var item in request.Items)
                {
                    var stock = stocks.First(s => s.ProductId == item.ProductId);

                    stock.Quantity -= item.Quantity;
                    await _stockRepository.UpdateAsync(stock.StockId, stock);

                    order.OrderDetails!.Add(new OrderDetails
                    {
                        OrderDetailsId = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductPrice = item.ProductPrice,
                        Quantity = item.Quantity
                    });
                }

                await _repository.AddAsync(order);

                var payment = new Payment
                {
                    UserId = request.UserId,
                    OrderId = order.OrderId,
                    TotalAmount = request.TotalAmount,
                    PaymentType = request.PaymentType
                };

                await _paymentRepository.AddAsync(payment);

                await _unitOfWork.CommitAsync();

                return new GetUserOrderDetailsResponseDTO
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Status = order.Status,
                    TotalProductsCount = order.TotalProductsCount,
                    TotalAmount = order.TotalAmount
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrder(Guid OrderId, string Status)
        {
            var order = await _repository.GetQueryable()
                .FirstOrDefaultAsync(o => o.OrderId == OrderId);

            if (order == null)
                return false;

            order.Status = Status;

            await _repository.UpdateAsync(order.OrderId, order);

            return true;
        }
    }
}
