using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Guid, Stock> _stockRepository;
        private readonly IRepository<Guid, Product> _productRepository;
        private readonly IRepository<Guid, Category> _categoryRepository;

        private readonly IUnitOfWork _unitOfWork;

        public ProductService(
            IRepository<Guid, Stock> stockRepository,
            IRepository<Guid, Product> productRepository,
            IRepository<Guid, Category> categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<GetAllProductsResponseDTO> AddProduct(AddNewProductRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();

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

                await _productRepository.AddAsync(product);

                var stock = new Stock
                {
                    ProductId = product.ProductId,
                    Quantity = request.Quantity
                };

                await _stockRepository.AddAsync(stock);

                await _unitOfWork.CommitAsync();

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
                await _unitOfWork.RollbackAsync();
                throw new Exception(ex.Message);
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

            return await query
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
                    Quantity = p.Stock.Quantity,
                    Review = (p.Reviews ?? new List<Review>())
                    .Select(r => new ReviewDTO
                    {
                        Summary = r.Summary,
                        ReviewPoints = r.ReviewPoints
                    })
                    .ToList()
                })
                .ToListAsync();
        }

        public async Task<GetAllProductsResponseDTO> SearchProductById(SearchProductByIdRequestDTO request)
        {
            var product = await _productRepository
                .GetQueryable()
                .Where(p => p.ProductId == request.ProductId)
                .Select(p => new GetAllProductsResponseDTO
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    StockId = p.Stock!.StockId,
                    Name = p.Name,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryName = p.Category!.CategoryName,
                    Quantity = p.Stock!.Quantity,
                    Review = (p.Reviews ?? new List<Review>())
                    .Select(r => new ReviewDTO
                    {
                        Summary = r.Summary,
                        ReviewPoints = r.ReviewPoints
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            if (product == null)
                throw new AppException("Product not found");

            return product;
        }

        public async Task<IEnumerable<GetAllProductsResponseDTO>> SearchProductByName(SearchProductRequestDTO request)
        {
            var searchText = request.ProductName.Trim();

            return await _productRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.Name, $"%{searchText}%"))
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
                    Quantity = p.Stock.Quantity,
                    Review = (p.Reviews ?? new List<Review>())
                    .Select(r => new ReviewDTO
                    {
                        Summary = r.Summary,
                        ReviewPoints = r.ReviewPoints
                    })
                    .ToList()
                })
                .ToListAsync();
        }

        public async Task<UpdateProductResponseDTO> UpdateProduct(UpdateProductRequestDTO request)
        {
            var product = await _productRepository
                .GetQueryable()
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

            if (product == null)
                throw new Exception("Product not found");

            var stock = await _stockRepository
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.ProductId == request.ProductId);

            if (stock == null)
                throw new Exception("Stock not found");

            var category = await _categoryRepository
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

            if (category == null)
                throw new Exception("Category not found");

            product.CategoryId = request.CategoryId;
            product.Name = request.Name;
            product.ImagePath = request.ImagePath;
            product.Description = request.Description;
            product.Price = request.Price;

            stock.Quantity = request.Quantity;

            await _productRepository.UpdateAsync(product.ProductId, product);
            await _stockRepository.UpdateAsync(stock.StockId, stock);

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