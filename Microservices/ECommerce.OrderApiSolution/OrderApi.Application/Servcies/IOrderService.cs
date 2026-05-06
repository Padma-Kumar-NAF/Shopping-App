using OrderApi.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderApi.Application.Servcies
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetOrdersByClientId(int clientId);
        Task<OrderDetailsDTO> GetOrderDetails(int orderId);
    }
}
