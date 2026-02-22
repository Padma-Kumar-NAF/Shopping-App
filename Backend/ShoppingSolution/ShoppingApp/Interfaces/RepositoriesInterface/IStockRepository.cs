using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface IStockRepository
    {
        public Task<IEnumerable<GetStockResponseDTO>> GetStockAsync(int pageNumber, int pageSize);
    }
}
