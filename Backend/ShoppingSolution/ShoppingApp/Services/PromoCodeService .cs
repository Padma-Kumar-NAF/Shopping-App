using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Promocode;

public class PromoCodeService : IPromoCodeService
{
    private readonly IRepository<Guid, PromoCode> _promoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PromoCodeService(
        IRepository<Guid, PromoCode> promoRepository,
        IUnitOfWork unitOfWork)
    {
        _promoRepository = promoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<AddPromoCodeResponseDTO>> AddPromoCode(AddPromoCodeRequestDTO request)
    {
        if (request.FromDate > request.ToDate)
        {
            throw new AppException("FromDate cannot be greater than ToDate", 400);
        }

        var promoName = await _promoRepository.GetQueryable().FirstOrDefaultAsync(p => p.PromoCodeName == request.PromoCodeName.Trim().ToUpper());
        if(promoName != null)
        {
            throw new AppException("Promo code already exist", 401);
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var promo = new PromoCode
            {
                PromoCodeName = request.PromoCodeName.Trim().ToUpper(),
                DiscountPercentage = request.DiscountPercentage,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                CreatedAt = DateTime.UtcNow
            };

            await _promoRepository.AddAsync(promo);
            await _unitOfWork.CommitAsync();

            return new ApiResponse<AddPromoCodeResponseDTO>
            {
                StatusCode = 200,
                Data = new AddPromoCodeResponseDTO
                {
                    PromoCodeId = promo.PromoCodeId
                },
                Message = "Promo code created successfully"
            };
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<EditPromocodeResponseDTO>> EditPromoCode(EditPromocodeRequestDTO request)
    {
        if (request.FromDate > request.ToDate)
        {
            throw new AppException("FromDate cannot be greater than ToDate", 400);
        }

        var promo = await _promoRepository.GetQueryable().FirstOrDefaultAsync(p => p.PromoCodeId == request.PromoCodeId);

        if (promo == null)
        {
            throw new AppException("Promo code not found", 404);
        }

        var existing = await _promoRepository
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.PromoCodeName == request.PromoCodeName.Trim().ToUpper()
                && p.PromoCodeId != request.PromoCodeId);

        if (existing != null)
        {
            throw new AppException("Promo code name already exists", 400);
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            promo.PromoCodeName = request.PromoCodeName.Trim().ToUpper();
            promo.DiscountPercentage = request.DiscountPercentage;
            promo.FromDate = request.FromDate;
            promo.ToDate = request.ToDate;

            await _promoRepository.UpdateAsync(request.PromoCodeId,promo);
            await _unitOfWork.CommitAsync();

            return new ApiResponse<EditPromocodeResponseDTO>
            {
                StatusCode = 200,
                Message = "Promo code updated successfully",
                Data = new EditPromocodeResponseDTO
                {
                    IsSuccess = true
                }
            };
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<GetAllPromocodeResponseDTO>> GetAllPromocode(GetAllPromocodeRequestDTO request)
    {
        var query = _promoRepository.GetQueryable().Where(p => !p.IsDeleted);

        var totalCount = await query.CountAsync();

        var promos = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync();

        var data = promos.Select(p => new PromoCodeItemDTO
        {
            PromoCodeId = p.PromoCodeId,
            PromoCodeName = p.PromoCodeName,
            DiscountPercentage = p.DiscountPercentage,
            FromDate = p.FromDate,
            ToDate = p.ToDate
        }).ToList();

        return new ApiResponse<GetAllPromocodeResponseDTO>
        {
            StatusCode = 200,
            Message = "Promo codes fetched successfully",
            Data = new GetAllPromocodeResponseDTO
            {
                PromoCodes = data,
                TotalCount = totalCount
            }
        };
    }

    public async Task<ApiResponse<DeletePromocodeResponseDTO>> DeletePromoCode(DeletePromocodeRequestDTO request)
    {
        if (request.PromoCodeId == Guid.Empty)
            throw new AppException("Invalid PromoCode Id", 400);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var promo = await _promoRepository.GetQueryable()
                .FirstOrDefaultAsync(p => p.PromoCodeId == request.PromoCodeId && !p.IsDeleted);

            if (promo == null)
                throw new AppException("Promo code not found", 404);

            promo.IsDeleted = true;
            await _promoRepository.UpdateAsync(promo.PromoCodeId, promo);
            await _unitOfWork.CommitAsync();

            return new ApiResponse<DeletePromocodeResponseDTO>
            {
                StatusCode = 200,
                Message = "Promo code deleted successfully",
                Data = new DeletePromocodeResponseDTO { IsSuccess = true }
            };
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<VerifyPromoCodeResponseDTO>> VerifyPromoCode(VerifyPromoCodeRequestDTO request)
    {
        var code = request.PromoCodeName.Trim().ToUpper();

        var promo = await _promoRepository.GetQueryable()
            .FirstOrDefaultAsync(p => p.PromoCodeName == code && !p.IsDeleted);

        if (promo == null)
        {
            return new ApiResponse<VerifyPromoCodeResponseDTO>
            {
                StatusCode = 200,
                Data = new VerifyPromoCodeResponseDTO
                {
                    IsValid = false,
                    Message = "Invalid promo code"
                }
            };
        }

        var now = DateTime.UtcNow.Date;

        if (now < promo.FromDate.Date)
        {
            return new ApiResponse<VerifyPromoCodeResponseDTO>
            {
                StatusCode = 200,
                Data = new VerifyPromoCodeResponseDTO
                {
                    IsValid = false,
                    Message = "Promo code not active yet"
                }
            };
        }

        if (now > promo.ToDate.Date)
        {
            return new ApiResponse<VerifyPromoCodeResponseDTO>
            {
                StatusCode = 200,
                Data = new VerifyPromoCodeResponseDTO
                {
                    IsValid = false,
                    Message = "Promo code expired"
                }
            };
        }

        return new ApiResponse<VerifyPromoCodeResponseDTO>
        {
            StatusCode = 200,
            Data = new VerifyPromoCodeResponseDTO
            {
                IsValid = true,
                DiscountPercentage = promo.DiscountPercentage,
                PromoCodeId = promo.PromoCodeId,
                Message = "Promo code applied successfully"
            },
            Message = "Promo code applied successfully"
        };
    }
}