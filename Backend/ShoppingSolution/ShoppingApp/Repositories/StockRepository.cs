using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Repositories
{
    public class StockRepository : Repository<Guid, Stock>, IStockRepository
    {
        public StockRepository(ShoppingContext context) : base(context)
        {

        }

        public async Task<Stock> AddNewStock(Stock newStock)
        {
            var result = await _context.Stock.AddAsync(newStock);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<IEnumerable<GetStockResponseDTO>> GetStockAsync(int Limit, int pageNumber)
        {
            var stocks = await _context.Stock
                .Include(s => s.Product)
                .ThenInclude(p => p.Category)
                .OrderBy(s => s.Product!.Name)
                .Skip((pageNumber - 1) * Limit)
                .Take(Limit)
                .Select(s => new GetStockResponseDTO
                {
                    StockId = s.StockId,
                    ProductId = s.ProductId,
                    ProductName = s.Product!.Name,
                    ProductImage = s.Product.ImagePath,
                    CategoryId = s.Product.CategoryId,
                    CategoryName = s.Product.Category!.CategoryName,
                    Price = s.Product.Price,
                    Quantity = s.Quantity
                })
                .ToListAsync();

            return stocks;
        }
    }
}
