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
            var cancelOrder = await _orderRepository.CancelOrder(request);
            return cancelOrder;
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
