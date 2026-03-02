using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Product;
using ShoppingApp.Repositories;
using System;

namespace ShoppingApp.Services
{
    public class ProductService : IProductService
    {
        private readonly ShoppingContext _context;
        IRepository<Guid, Stock> _stockRepository;
        IRepository<Guid, Product> _productRepository;
        public ProductService(ShoppingContext context, IRepository<Guid, Stock> stockRepository, IRepository<Guid, Product> productRepository)
        {
            _context = context;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        public async Task<GetAllProductsResponseDTO> AddProduct(AddNewProductRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var product = new Product
                {
                    CategoryId = request.CategoryId,
                    Name = request.Name,
                    ImagePath = request.ImagePath,
                    Description = request.Description,
                    Price = request.Price,
                };

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                var stock = new Stock
                {
                    ProductId = product.ProductId, 
                    Quantity = request.Quantity
                };

                await _context.Stock.AddAsync(stock);
                await _context.SaveChangesAsync(); 

                await transaction.CommitAsync();

                return new GetAllProductsResponseDTO
                {
                    ProductId = product.ProductId,
                    CategoryId = product.CategoryId,
                    Name = product.Name,
                    ImagePath = product.ImagePath,
                    Description = product.Description,
                    Price = product.Price,
                    Quantity = stock.Quantity,
                    StockId = stock.StockId
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request)
        {
            var query = _productRepository.GetQueryable().AsNoTracking();

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.Limit <= 0)
                request.Limit = 10;

            if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
            {
                query = query
                    .Where(p => p.CategoryId == request.CategoryId.Value)
                    .OrderBy(p => p.Name);
            }
            else
            {
                var totalCount = await query.CountAsync();

                if (totalCount == 0)
                    return new List<GetAllProductsResponseDTO>();

                var random = new Random();

                int maxSkip = Math.Max(0, totalCount - request.Limit);
                int randomSkip = random.Next(0, maxSkip + 1);

                query = query
                    .OrderBy(p => p.Name)
                    .Skip(randomSkip);
            }

            var products = await query
                .Select(p => new GetAllProductsResponseDTO
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    CategoryName = p.Category!.CategoryName,
                    Price = p.Price,
                    StockId = p.Stock!.StockId,
                    Quantity = p.Stock.Quantity
                })
                .Skip((request.PageNumber - 1) * request.Limit)
                .Take(request.Limit)
                .ToListAsync();

            return products;
        }

        public async Task<IEnumerable<GetAllProductsResponseDTO>> SearchProductByName(SearchProductRequestDTO request)
        {
            var searchText = request.ProductName.Trim();

            var query = _productRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.Name, $"%{searchText}%"));

            var result = await query
                .OrderBy(p => p.Name)
                .Skip((request.PageNumber - 1) * request.Limit)
                .Take(request.Limit)
                .Select(p => new GetAllProductsResponseDTO
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    CategoryName = p.Category!.CategoryName,
                    Price = p.Price,
                    StockId = p.Stock!.StockId,
                    Quantity = p.Stock.Quantity
                })
                .ToListAsync();

            return result;
        }

        public async Task<UpdateProductResponseDTO> UpdateProduct(UpdateProductRequestDTO request)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

            if (product == null)
                throw new Exception("Product not found");

            var stock = await _context.Stock
                .FirstOrDefaultAsync(s => s.ProductId == request.ProductId);

            if (stock == null)
                throw new Exception("Stock not found");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

            if (category == null)
                throw new Exception("Category not found");

            product.CategoryId = request.CategoryId;
            product.Name = request.Name;
            product.ImagePath = request.ImagePath;
            product.Description = request.Description;
            product.Price = request.Price;

            stock.Quantity = request.Quantity;

            await _context.SaveChangesAsync();

            return new UpdateProductResponseDTO
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                StockId = stock.StockId,
                Name = product.Name,
                ImagePath = product.ImagePath,
                Description = product.Description,
                CategoryName = category.CategoryName,
                Price = product.Price,
                Quantity = stock.Quantity
            };
        }
    }
}