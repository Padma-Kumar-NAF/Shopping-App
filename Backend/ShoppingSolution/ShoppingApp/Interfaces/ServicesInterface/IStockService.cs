using ShoppingApp.Models.DTOs.Product;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IStockService
    {
        public Task<IEnumerable<GetStockResponseDTO>> GetStock(GetStockRequestDTO request);
    }
}
