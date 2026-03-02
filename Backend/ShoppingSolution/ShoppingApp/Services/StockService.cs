//using Microsoft.EntityFrameworkCore;
//using ShoppingApp.Contexts;
//using ShoppingApp.Interfaces.RepositoriesInterface;
//using ShoppingApp.Interfaces.ServicesInterface;
//using ShoppingApp.Models;
//using ShoppingApp.Models.DTOs.Stock;

//namespace ShoppingApp.Services
//{
//    public class StockService : IStockService
//    {
//        private readonly ShoppingContext _context;
//        public StockService(ShoppingContext context)
//        {
//            _context = context;
//        }

//        public async Task<IEnumerable<GetStockResponseDTO>> GetStock(GetStockRequestDTO request)
//        {
//            var stocks = await _context.Stock
//                .Include(s => s.Product)
//                .ThenInclude(p => p.Category)
//                .OrderBy(s => s.Product!.Name)
//                .Skip((request.PageNumber - 1) * request.Limit)
//                .Take(request.Limit)
//                .Select(s => new GetStockResponseDTO
//                {
//                    StockId = s.StockId,
//                    ProductId = s.ProductId,
//                    ProductName = s.Product!.Name,
//                    ProductImage = s.Product.ImagePath,
//                    CategoryId = s.Product.CategoryId,
//                    CategoryName = s.Product.Category!.CategoryName,
//                    Price = s.Product.Price,
//                    Quantity = s.Quantity
//                })
//                .ToListAsync();

//            return stocks;
//        }

//        public async Task<AddNewStockResponseDTO> AddStock(AddNewStockRequestDTO request)
//        {
//            try
//            {
//                var productExists = await _context.Products
//                    .AnyAsync(p => p.ProductId == request.ProductId);

//                if (!productExists)
//                    throw new Exception("Product does not exist");

//                var existingStock = await _context.Stock
//                    .FirstOrDefaultAsync(s => s.ProductId == request.ProductId);

//                if (existingStock != null)
//                {
//                    existingStock.Quantity += request.Quantity;

//                    await _context.SaveChangesAsync();

//                    return new AddNewStockResponseDTO
//                    {
//                        StockId = existingStock.StockId,
//                        ProductId = existingStock.ProductId,
//                        Quantity = existingStock.Quantity
//                    };
//                }

//                var stock = new Stock
//                {
//                    ProductId = request.ProductId,
//                    Quantity = request.Quantity
//                };

//                await _context.Stock.AddAsync(stock);
//                await _context.SaveChangesAsync();

//                return new AddNewStockResponseDTO
//                {
//                    StockId = stock.StockId,
//                    ProductId = stock.ProductId,
//                    Quantity = stock.Quantity
//                };
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }
//    }
//}
