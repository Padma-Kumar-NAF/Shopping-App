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
        
        private readonly ShoppingContext _context;

        public OrderService(IRepository<Guid, Order> repository, ShoppingContext context, IRepository<Guid, Stock> stockRepository)
        {
            _repository = repository;
            _stockRepository = stockRepository;

            _context = context;
        }

        public async Task<GetUserOrderDetailsResponseDTO> CancelOrder(CancelOrderRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails!)
                    .Include(o => o.Address)
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

                if (order == null)
                    throw new Exception("Order not found.");

                if (order.Status == "Cancelled")
                    throw new Exception("Order already cancelled.");

                if (order.Status == "Shipped" || order.Status == "Delivered")
                    throw new Exception("Cannot cancel shipped/delivered order.");

                foreach (var item in order.OrderDetails!)
                {
                    var stock = await _stockRepository
                        .GetQueryable()
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                    if (stock == null)
                        throw new Exception($"Stock not found for product {item.ProductId}");

                    stock.Quantity += item.Quantity;

                    await _stockRepository.UpdateAsync(stock.StockId, stock);
                }

                order.Status = "Cancelled";
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<GetUserOrderDetailsResponseDTO>> GetUserOrderById(GetUserOrderDetailsRequestDTO request)
        {
            var query = _repository.GetQueryable()
                .Where(order => order.UserId == request.UserId)
                .OrderByDescending(o => o.CreatedAt);

            var pagedOrders = await query
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

                    Items = o.OrderDetails!
                        .Select(od => new OrderDetailsDTO
                        {
                            OrderDetailsId = od.OrderDetailsId,
                            ProductId = od.ProductId,
                            ProductName = od.ProductName,
                            Quantity = od.Quantity,
                            ProductPrice = od.ProductPrice,
                            ImagePath = od.Product!.ImagePath
                        }).OrderBy(od => od.ProductName)
                        .ToList()
                })
                .ToListAsync();

            return pagedOrders;
        }

        public async Task<GetUserOrderDetailsResponseDTO> PlaceOrder(PlaceOrderRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var productIds = request.Items.Select(i => i.ProductId).ToList();

                var stocks = await _context.Stock
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
                    DeliveryDate = DateTime.Now,
                    OrderDetails = new List<OrderDetails>()
                };

                foreach (var item in request.Items)
                {
                    var stock = stocks.First(s => s.ProductId == item.ProductId);

                    stock.Quantity -= item.Quantity;

                    order.OrderDetails.Add(new OrderDetails
                    {
                        OrderDetailsId = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductPrice = item.ProductPrice,
                        Quantity = item.Quantity
                    });
                }

                await _context.Orders.AddAsync(order);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var createdOrder = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderId == order.OrderId)
                    .Select(o => new GetUserOrderDetailsResponseDTO
                    {
                        OrderId = o.OrderId,
                        UserId = o.UserId,
                        Status = o.Status,
                        TotalProductsCount = o.TotalProductsCount,
                        TotalAmount = o.TotalAmount,

                        Address = new AddressDTO
                        {
                            AddressId = o.Address!.AddressId,
                            AddressLine1 = o.Address.AddressLine1,
                            AddressLine2 = o.Address.AddressLine2,
                            State = o.Address.State,
                            City = o.Address.City,
                            Pincode = o.Address.Pincode
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
                    .FirstOrDefaultAsync();

                if (createdOrder == null)
                    throw new Exception("Order created but unable to fetch details.");

                return createdOrder;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrder(Guid OrderId, string Status)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == OrderId);

            if (order == null)
                return false;

            order.Status = Status;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
