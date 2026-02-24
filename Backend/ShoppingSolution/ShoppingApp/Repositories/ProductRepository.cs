using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Repositories
{
    public class ProductRepository : Repository<Guid,Product> , IProductRepository
    {
        //IRepository<Guid, Product> _repository;
        IRepository<Guid, Stock> _stockRepository;
        public ProductRepository( IRepository<Guid, Stock> stockRepository ,ShoppingContext context) : base(context) 
        {
            //_repository = repository;
            _stockRepository = stockRepository;
        }

        public async Task<GetAllProductsResponseDTO> AddProduct(AddNewProductRequestDTO request)
        {
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

                var addedProduct = await base.AddAsync(product);

                var stock = new Stock
                {
                    ProductId = product.ProductId,
                    Quantity = request.Quantity
                };

                var addedStock = await _stockRepository.AddAsync(stock);

                await _context.SaveChangesAsync();

                return new GetAllProductsResponseDTO
                {
                    ProductId = addedProduct.ProductId,
                    CategoryId = product.CategoryId,
                    Name = product.Name,
                    ImagePath = product.ImagePath,
                    Description = product.Description,
                    Price = product.Price,
                    Quantity = stock.Quantity,
                    StockId = stock.StockId
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request)
        {
            var query = base.GetQueryable().AsNoTracking();

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

        public async Task<IEnumerable<GetAllProductsResponseDTO>> SearchProducts(SearchProductRequestDTO request)
        {
            var result = new List<GetAllProductsResponseDTO>();

            var query = base.GetQueryable().AsNoTracking();

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.Limit <= 0)
                request.Limit = 10;

            if (request.ProductId != Guid.Empty)
            {
                var searchedProduct = await query
                    .Where(p => p.ProductId == request.ProductId)
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
                    .FirstOrDefaultAsync();

                if (searchedProduct != null)
                    result.Add(searchedProduct);
            }

            if (request.CategoryId == Guid.Empty)
                return result;

            var categoryRequest = new GetAllProductsRequestDTO
            {
                CategoryId = request.CategoryId,
                PageNumber = request.PageNumber,
                Limit = request.Limit
            };

            var categoryProducts = await GetProducts(categoryRequest);


            var filteredCategoryProducts = categoryProducts // -> This is for remove duplicates
                .Where(p => p.ProductId != request.ProductId);

            result.AddRange(filteredCategoryProducts);

            return result;
        }
    }
}
