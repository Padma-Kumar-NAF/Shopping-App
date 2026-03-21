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
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Error while adding product",ex,500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while adding product",ex,500);
            }
        }

        public async Task<ApiResponse<GetAllProductsResponseDTO>> GetProducts(GetAllProductsRequestDTO request)
        {
            try
            {
                var query = _productRepository.GetQueryable().AsNoTracking();

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
                    .Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
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
                        Review = (p.Reviews ?? new List<Review>())
                            .Select(r => new ReviewDTO
                            {
                                Summary = r.Summary,
                                ReviewPoints = r.ReviewPoints
                            })
                            .ToList()
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
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while Fetching product", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while Fetching product", ex, 500);
            }
        }

        public async Task<ApiResponse<SearchProductByIdResponseDTO>> SearchProductById(SearchProductByIdRequestDTO request)
        {
            try
            {
                var product = await _productRepository
                .GetQueryable()
                .Where(p => p.ProductId == request.ProductId)
                .Select(p => new SearchProductByIdResponseDTO
                {
                    ProductId = p.ProductId,
                    CategoryId = p.CategoryId,
                    StockId = p.Stock!.StockId,
                    ProductName = p.Name,
                    ImagePath = p.ImagePath,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryName = p.Category!.CategoryName,
                    Quantity = p.Stock.Quantity,
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
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while Searching product", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while Searching product", ex, 500);
            }
        }

        public async Task<ApiResponse<SearchProductByNameResponseDTO>> SearchProductByName(SearchProductByNameRequestDTO request)
        {
            try
            {
                var searchText = request.ProductName.Trim();

                var products = await _productRepository
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(p => EF.Functions.Like(p.Name, $"%{searchText}%"))
                    .OrderBy(p => p.Name)
                    .Select(p => new ProductDetailsDTO
                    {
                        ProductId = p.ProductId,
                        CategoryId = p.CategoryId,
                        StockId = p.Stock!.StockId,
                        Name = p.Name,
                        ImagePath = p.ImagePath,
                        Description = p.Description,
                        CategoryName = p.Category!.CategoryName,
                        Price = p.Price,
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
                    ProductsList = products
                };

                return new ApiResponse<SearchProductByNameResponseDTO>()
                {
                    Data = response,
                    StatusCode = 200,
                    Message = "Products fetched successfully"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while Searching product", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while Searching product", ex, 500);
            }
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
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Error while Searching product", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while Searching product", ex, 500);
            }
        }
        private async Task<bool> IsUserNotFound(Guid userId)
        {
            var user = await _userRepository.GetAsync(userId);
            return user == null;
        }
    }
}