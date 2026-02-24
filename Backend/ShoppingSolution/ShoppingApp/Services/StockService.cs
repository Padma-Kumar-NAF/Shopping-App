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

        public async Task<AddNewStockResponseDTO> AddStock(AddNewStockRequestDTO request)
        {
            try
            {
                Stock stock = new Stock();
                stock.ProductId = request.ProductId;
                stock.Quantity = request.Quantity;

                var addedStock = await _stockRepository.AddNewStock(stock);

                AddNewStockResponseDTO newStock = new AddNewStockResponseDTO();

                newStock.StockId = addedStock.StockId;
                newStock.ProductId = stock.ProductId;
                newStock.Quantity = stock.Quantity;

                return newStock;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
