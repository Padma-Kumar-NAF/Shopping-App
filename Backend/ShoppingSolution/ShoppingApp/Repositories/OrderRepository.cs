using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Repositories
{
    public class OrderRepository : Repository<Guid, Order>, IOrderRepository
    {
        private readonly IRepository<Guid, Stock> _stockRepository;
        private readonly IRepository<Guid, OrderDetails> _orderDetailsRepository;

        public OrderRepository(
            ShoppingContext context,
            IRepository<Guid, Stock> stockRepository,
            IRepository<Guid, OrderDetails> orderDetailsRepository
        ) : base(context)
        {
            _stockRepository = stockRepository;
            _orderDetailsRepository = orderDetailsRepository;
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

        public async Task<GetUserOrderDetailsResponseDTO> PlaceOrder(PlaceOrderRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = request.UserId,
                    Status = "Not Delivered",
                    TotalAmount = request.TotalAmount,
                    TotalProductsCount = request.TotalProductsCount,
                    AddressId = request.AddressId,
                };

                var addedOrder = await base.AddAsync(order);

                if (addedOrder == null)
                    throw new Exception("Unable to place order");

                foreach (var item in request.Items)
                {
                    var stock = await _stockRepository
                        .GetQueryable()
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                    if (stock == null)
                        throw new Exception($"Stock not found for product {item.ProductId}");

                    if (stock.Quantity < item.Quantity)
                        throw new Exception($"Insufficient stock for product {item.ProductName}");

                    stock.Quantity -= item.Quantity;
                    await _stockRepository.UpdateAsync(stock.StockId, stock);

                    var orderDetails = new OrderDetails
                    {
                        OrderId = addedOrder.OrderId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductPrice = item.ProductPrice,
                        Quantity = item.Quantity,
                    };

                    var addedOrderDetails = await _orderDetailsRepository.AddAsync(orderDetails);

                    if (addedOrderDetails == null)
                        throw new Exception("Unable to add order details");
                }

                var createdOrder = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderId == addedOrder.OrderId)
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
                    throw new Exception("Order not found after creation");

                await transaction.CommitAsync();

                return createdOrder;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}