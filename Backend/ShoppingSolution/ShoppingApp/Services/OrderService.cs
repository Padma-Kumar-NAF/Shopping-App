using Microsoft.EntityFrameworkCore;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Services
{
    public class OrderService : IOrderService
    {
        IRepository<Guid, Order> _repository;
        IOrderRepository _orderRepository;
        public OrderService(IRepository<Guid, Order> repository, IOrderRepository orderRepository)
        {
            _repository = repository;
            _orderRepository = orderRepository;
        }

        public async Task<GetUserOrderDetailsResponseDTO> CancelOrder(CancelOrderRequestDTO request)
        {
            var order = await _repository.GetQueryable()
                .Include(o => o.Address)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId);

            if (order == null)
                throw new Exception("Order not found.");

            if (order.Status == "Cancelled")
                throw new Exception("Order is already cancelled.");

            if (order.Status == "Shipped" || order.Status == "Delivered")
                throw new Exception("Shipped or delivered orders cannot be cancelled.");

            order.Status = "Cancelled";

            await _repository.UpdateAsync(order.OrderId, order);
            
            return new GetUserOrderDetailsResponseDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                Status = order.Status,
                TotalProductsCount = order.TotalProductsCount,
                TotalAmount = order.TotalAmount,

                Address = new AddressDTO
                {
                    AddressId = order.Address!.AddressId,
                    AddressLine1 = order.Address.AddressLine1,
                    AddressLine2 = order.Address.AddressLine2,
                    State = order.Address.State,
                    City = order.Address.City,
                    Pincode = order.Address.Pincode
                },

                Items = order.OrderDetails!
                    .Select(od => new OrderDetailsDTO
                    {
                        OrderDetailsId = od.OrderDetailsId,
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        ProductPrice = od.ProductPrice,
                        ImagePath = od.Product!.ImagePath
                    })
                    .OrderBy(i => i.ProductName)
                    .ToList()
            };
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
            try
            {
                var order = await _orderRepository.PlaceOrder(request);
                return order;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
