using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.Services;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.UserMonthlyProductLimit;
using ShoppingApp.Models.Entities;

namespace ShoppingApp.Services
{
    public class UserMonthlyProductLimitService : IUserMonthlyProductLimit
    {
        private readonly IRepository<Guid, UserMonthlyProductLimit> _repository;
        private readonly IRepository<Guid, Product> _productRepository;

        public UserMonthlyProductLimitService(
            IRepository<Guid, UserMonthlyProductLimit> repository,
            IRepository<Guid, Product> productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
        }

        public async Task<ApiResponse<AddUserMonthlyProductLimitResponseDTO>> AddLimit(AddUserMonthlyProductLimitRequestDTO request)
        {
            var product = await _productRepository.GetAsync(request.ProductId);

            if (product == null)
            {
                throw new AppException("Product not found", 404);
            }

            var exists = await _repository.GetQueryable()
                .AnyAsync(u => u.ProductId == request.ProductId);

            if (exists)
            {
                throw new AppException("A monthly limit for this product already exists", 409);
            }

            var newLimit = new UserMonthlyProductLimit
            {
                ProductId = request.ProductId,
                MonthlyLimit = request.MonthlyLimit,
            };

            var result = await _repository.AddAsync(newLimit);

            if (result == null)
            {
                throw new AppException("Unable to add monthly product limit at this moment", 500);
            }

            return new ApiResponse<AddUserMonthlyProductLimitResponseDTO>
            {
                StatusCode = 200,
                Message = "Monthly product limit added successfully",
                Data = new AddUserMonthlyProductLimitResponseDTO { Id = result.Id },
                Action = "AddLimit"
            };
        }

        public async Task<ApiResponse<EditUserMonthlyProductLimitResponseDTO>> EditLimit(EditUserMonthlyProductLimitRequestDTO request)
        {
            var limit = await _repository.GetQueryable().FirstOrDefaultAsync(u => u.Id == request.Id);

            if (limit == null)
            {
                throw new AppException("Monthly product limit not found", 404);
            }                

            if (limit.MonthlyLimit == request.MonthlyLimit)
            {
                return new ApiResponse<EditUserMonthlyProductLimitResponseDTO>
                {
                    StatusCode = 200,
                    Message = "No changes required",
                    Data = new EditUserMonthlyProductLimitResponseDTO { 
                        IsSuccess = true
                    },
                    Action = "EditLimit"
                };
            }

            limit.MonthlyLimit = request.MonthlyLimit;

            await _repository.UpdateAsync(request.Id, limit);

            return new ApiResponse<EditUserMonthlyProductLimitResponseDTO>
            {
                StatusCode = 200,
                Message = "Monthly product limit updated successfully",
                Data = new EditUserMonthlyProductLimitResponseDTO { 
                    IsSuccess = true 
                },
                Action = "EditLimit"
            };
        }

        public async Task<ApiResponse<DeleteUserMonthlyProductLimitResponseDTO>> DeleteLimit(DeleteUserMonthlyProductLimitRequestDTO request)
        {
            var limit = await _repository.FirstOrDefaultAsync(u => u.Id == request.Id);

            if (limit == null)
            {
                throw new AppException("Monthly product limit not found", 404);
            }

            var deleted = await _repository.DeleteAsync(request.Id);

            if (deleted == null)
            {
                throw new AppException("Failed to delete monthly product limit", 500);
            }

            return new ApiResponse<DeleteUserMonthlyProductLimitResponseDTO>
            {
                StatusCode = 200,
                Message = "Monthly product limit deleted successfully",
                Data = new DeleteUserMonthlyProductLimitResponseDTO { IsSuccess = true },
                Action = "DeleteLimit"
            };
        }

        public async Task<ApiResponse<GetAllUserMonthlyProductLimitResponseDTO>> GetAllLimits(GetAllUserMonthlyProductLimitRequestDTO request)
        {
            var query = _repository.GetQueryable().AsNoTracking().Include(u => u.Product);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new ApiResponse<GetAllUserMonthlyProductLimitResponseDTO>
                {
                    StatusCode = 200,
                    Message = "No records found",
                    Data = new GetAllUserMonthlyProductLimitResponseDTO
                    {
                        Records = new List<UserMonthlyProductLimitDTO>(),
                        TotalCount = 0,
                        PageNumber = request.Pagination.PageNumber,
                        PageSize = request.Pagination.PageSize
                    },
                    Action = "GetAllLimits"
                };
            }

            var records = await query
                .OrderBy(u => u.CreatedAt)
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(u => new UserMonthlyProductLimitDTO
                {
                    Id = u.Id,
                    ProductId = u.ProductId,
                    ProductName = u.Product != null ? u.Product.Name : string.Empty,
                    MonthlyLimit = u.MonthlyLimit,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return new ApiResponse<GetAllUserMonthlyProductLimitResponseDTO>
            {
                StatusCode = 200,
                Message = "Records fetched successfully",
                Data = new GetAllUserMonthlyProductLimitResponseDTO
                {
                    Records = records,
                    TotalCount = totalCount,
                    PageNumber = request.Pagination.PageNumber,
                    PageSize = request.Pagination.PageSize
                },
                Action = "GetAllLimits"
            };
        }
    }
}
