using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        public StockService(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<IEnumerable<GetStockResponseDTO>> GetStock(GetStockRequestDTO request)
        {
            var stocks = await _stockRepository.GetStockAsync(request.Limit,request.PageNumber);
            if(stocks == null)
            {
                throw new Exception("No Stocks found");
            }
            return stocks;
        }
    }
}
