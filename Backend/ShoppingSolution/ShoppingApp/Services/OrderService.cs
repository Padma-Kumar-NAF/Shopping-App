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
        IRepository<Guid, OrderDetails> _orderDetailsRepository;
        public OrderService(IRepository<Guid, Order> repository, IRepository<Guid, OrderDetails> orderDetailsRepository)
        {
            _repository = repository;
            _orderDetailsRepository = orderDetailsRepository;
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
            Order order = new Order
            {
                UserId = request.UserId,
                Status = "Not Delivered",
                TotalAmount = request.TotalAmount,
                TotalProductsCount = request.TotalProductsCount,
                AddressId = request.AddressId,
                CreatedAt = DateTime.UtcNow
            };

            var addedOrder = await _repository.AddAsync(order);

            if (addedOrder == null)
                throw new Exception("Unable to place order, Try after sometime");

            foreach (var item in request.Items)
            {
                OrderDetails orderDetails = new OrderDetails
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

            var createdOrder = await _repository.GetQueryable()
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
                .FirstAsync();

            return createdOrder;
        }
    }
}
