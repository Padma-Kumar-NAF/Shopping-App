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
        private readonly IRepository<Guid, User> _userRepository;

        private readonly IUnitOfWork _unitOfWork;

        public ProductService(
            IRepository<Guid, Stock> stockRepository,
            IRepository<Guid, Product> productRepository,
            IRepository<Guid, Category> categoryRepository,
            IRepository<Guid, User> userRepository,
            IUnitOfWork unitOfWork)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<AddNewProductResponseDTO>> AddProduct(Guid userId,AddNewProductRequestDTO request)
        {
            if (await IsUserNotFound(userId))
            {
                throw new AppException("User not found", 404);
            }

            var Category = await _categoryRepository.GetAsync(request.CategoryId);

            if(Category == null)
            {
                throw new AppException("Category not found",404);
            }

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
                    ActiveStatus = true
                };

                await _productRepository.AddAsync(product);

                var stock = new Stock
                {
                    ProductId = product.ProductId,
                    Quantity = request.Quantity
                };

                await _stockRepository.AddAsync(stock);

                await _unitOfWork.CommitAsync();

                return new ApiResponse<AddNewProductResponseDTO>()
                {
                    StatusCode = 200,
                    Data = new AddNewProductResponseDTO()
                    {
                        ProductId = product.ProductId
                    }
                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ApiResponse<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request)
        {
            var query = _productRepository.GetQueryable().AsNoTracking()
                .Where(p => p.ActiveStatus == true);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new ApiResponse<GetAllProductsResponseDTO>()
                {
                    Data = new GetAllProductsResponseDTO(),
                    StatusCode = 200,
                    Action = "ShowEmptyPage",
                    Message = "No products available for this moment"
                };
            }

            query = query.OrderBy(p => p.Name);

            var pageNumber = request.pagination.PageNumber;
            var pageSize = request.pagination.PageSize;

            var products = await query
                .Where(p => p.ActiveStatus)
                .Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
                .Take(request.pagination.PageSize)
                .Select(p => new ProductDetails
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    ProductName = p.Name,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    CategoryName = p.Category!.CategoryName,
                    Price = p.Price,
                    StockId = p.Stock!.StockId,
                    Quantity = p.Stock.Quantity,
                    Review = p.Reviews != null
                        ? p.Reviews.Select(r => new ReviewDTO
                        {
                            Summary = r.Summary,
                            ReviewPoints = r.ReviewPoints
                        }).ToList()
                        : new List<ReviewDTO>()
                })
                .ToListAsync();

            var response = new GetAllProductsResponseDTO
            {
                ProductList = products
            };

            return new ApiResponse<GetAllProductsResponseDTO>()
            {
                Data = response,
                StatusCode = 200,
                Message = "Products fetched successfully"
            };
        }

        public async Task<ApiResponse<GetAllProductsWithFilterResponseDTO>> GetProductsWithFilter(GetAllProductsWithFilterRequestDTO request)
        {
            if (request.LowPrice > request.HighPrice)
            {
                throw new AppException("Low price cannot be greater than high price", 400);
            }

            var query = _productRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(p => p.Price >= request.LowPrice && p.Price <= request.HighPrice);

            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            }

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new ApiResponse<GetAllProductsWithFilterResponseDTO>()
                {
                    Data = new GetAllProductsWithFilterResponseDTO(),
                    StatusCode = 200,
                    Action = "ShowEmptyPage",
                    Message = "No products found for the selected price range"
                };
            }

            query = query.OrderBy(p => p.Price);

            var pageNumber = request.pagination.PageNumber;
            var pageSize = request.pagination.PageSize;

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDetails
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    ProductName = p.Name,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    CategoryName = p.Category!.CategoryName,
                    Price = p.Price,
                    StockId = p.Stock!.StockId,
                    Quantity = p.Stock.Quantity,
                    Review = p.Reviews != null
                        ? p.Reviews.Select(r => new ReviewDTO
                        {
                            Summary = r.Summary,
                            ReviewPoints = r.ReviewPoints
                        }).ToList()
                        : new List<ReviewDTO>()
                })
                .ToListAsync();

            var response = new GetAllProductsWithFilterResponseDTO
            {
                ProductList = products
            };

            return new ApiResponse<GetAllProductsWithFilterResponseDTO>()
            {
                Data = response,
                StatusCode = 200,
                Message = "Filtered products fetched successfully",
                Action = "ShowProducts"
            };
        }

        public async Task<ApiResponse<List<ProductSuggestionDTO>>> GetSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ApiResponse<List<ProductSuggestionDTO>>()
                {
                    Data = new List<ProductSuggestionDTO>(),
                    StatusCode = 200,
                    Message = "Empty query",
                    Action = "ShowEmpty"
                };
            }

            query = query.Trim();

            var suggestions = await _productRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.Name, $"{query}%"))
                .OrderBy(p => p.Name)
                .Select(p => new ProductSuggestionDTO
                {
                    Name = p.Name
                })
                .Take(10)
                .ToListAsync();

            return new ApiResponse<List<ProductSuggestionDTO>>()
            {
                Data = suggestions,
                StatusCode = 200,
                Message = suggestions.Any()
                    ? "Suggestions fetched successfully"
                    : "No suggestions found",
                Action = suggestions.Any() ? "ShowList" : "ShowEmpty"
            };
        }

        public async Task<ApiResponse<SearchProductByIdResponseDTO>> SearchProductById(SearchProductByIdRequestDTO request)
        {
            var productEntity = await _productRepository
                .GetQueryable()
                .Include(p => p.Reviews)
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

            if (productEntity == null)
            {
                throw new AppException("Product not found", 404);
            }

            var product = new SearchProductByIdResponseDTO
            {
                ProductId = productEntity.ProductId,
                CategoryId = productEntity.CategoryId,
                StockId = productEntity.Stock!.StockId,
                ProductName = productEntity.Name,
                ImagePath = productEntity.ImagePath,
                Description = productEntity.Description,
                Price = productEntity.Price,
                CategoryName = productEntity.Category!.CategoryName,
                Quantity = productEntity.Stock.Quantity,
                Review = productEntity.Reviews?
                    .Select(r => new ReviewDTO
                    {
                        Summary = r.Summary,
                        ReviewPoints = r.ReviewPoints
                    })
                    .ToList() ?? new List<ReviewDTO>()
            };

            if (product == null)
            {
                throw new AppException("Product not found", 404);
            }

            return new ApiResponse<SearchProductByIdResponseDTO>()
            {
                Data = product,
                StatusCode = 200,
                Message = "Product fetched successfully"
            };
        }

        public async Task<ApiResponse<SearchProductByNameResponseDTO>> SearchProductByName(SearchProductByNameRequestDTO request)
        {
            var searchText = request.ProductName.Trim();

            var products = await _productRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.Name, $"%{searchText}%"))
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.ProductId,
                    p.CategoryId,
                    p.Name,
                    p.ImagePath,
                    p.Description,
                    p.Price,
                    CategoryName = p.Category!.CategoryName,
                    StockId = p.Stock!.StockId,
                    Quantity = p.Stock.Quantity,
                    Reviews = p.Reviews
                })
                .ToListAsync();

            var result = products.Select(p => new ProductDetailsDTO
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                StockId = p.StockId,
                Name = p.Name,
                ImagePath = p.ImagePath,
                Description = p.Description,
                CategoryName = p.CategoryName,
                Price = p.Price,
                Quantity = p.Quantity,
                Review = p.Reviews?
                    .Select(r => new ReviewDTO
                    {
                        Summary = r.Summary,
                        ReviewPoints = r.ReviewPoints
                    }).ToList() ?? new List<ReviewDTO>()
            }).ToList();

            if (!products.Any())
            {
                return new ApiResponse<SearchProductByNameResponseDTO>()
                {
                    Data = new SearchProductByNameResponseDTO(),
                    StatusCode = 200,
                    Message = "No matching products found",
                    Action = "ShowEmptyPage"
                };
            }

            var response = new SearchProductByNameResponseDTO
            {
                ProductsList = result
            };

            return new ApiResponse<SearchProductByNameResponseDTO>()
            {
                Data = response,
                StatusCode = 200,
                Message = "Products fetched successfully"
            };
        }

        public async Task<ApiResponse<UpdateProductResponseDTO>> UpdateProduct(Guid userId, UpdateProductRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (await IsUserNotFound(userId))
                {
                    throw new AppException("User not found", 404);
                }

                var product = await _productRepository.GetQueryable().FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

                if (product == null)
                {
                    throw new AppException("Product not found", 404);
                }

                var stock = await _stockRepository.GetQueryable().FirstOrDefaultAsync(s => s.ProductId == request.ProductId);

                if (stock == null)
                {
                    throw new AppException("Stock not found", 404);
                }

                var category = await _categoryRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

                if (category == null)
                {
                    throw new AppException("Category not found", 404);
                }

                product.CategoryId = request.CategoryId;
                product.Name = request.Name;
                product.ImagePath = request.ImagePath;
                product.Description = request.Description;
                product.Price = request.Price;

                stock.Quantity = request.Quantity;

                await _productRepository.UpdateAsync(product.ProductId, product);
                await _stockRepository.UpdateAsync(stock.StockId, stock);

                await _unitOfWork.CommitAsync();

                return new ApiResponse<UpdateProductResponseDTO>()
                {
                    Data = new UpdateProductResponseDTO
                    {
                        IsUpdate = true,
                        CategoryId = product.CategoryId
                    },
                    StatusCode = 200,
                    Message = "Product updated successfully"
                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ApiResponse<DeleteProductResponseDTO>> DeleteProduct(Guid userId, DeleteProductRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (await IsUserNotFound(userId))
                {
                    throw new AppException("User not found", 404);
                }

                var product = await _productRepository.GetQueryable()
                    .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

                if (product == null)
                {
                    throw new AppException("Product not found", 404);
                }

                product.ActiveStatus = false;

                await _productRepository.UpdateAsync(product.ProductId, product);
                await _unitOfWork.CommitAsync();

                return new ApiResponse<DeleteProductResponseDTO>()
                {
                    Data = new DeleteProductResponseDTO { IsDeleted = true },
                    StatusCode = 200,
                    Message = "Product deleted successfully"
                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        private async Task<bool> IsUserNotFound(Guid userId)
        {
            var user = await _userRepository.GetAsync(userId);
            return user == null;
        }
    }
}