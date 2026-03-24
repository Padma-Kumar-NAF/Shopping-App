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
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new AppException("Error while creating promo code", ex, 500);
        }
    }

    public async Task<ApiResponse<VerifyPromoCodeResponseDTO>> VerifyPromoCode(VerifyPromoCodeRequestDTO request)
    {
        try
        {
            var code = request.PromoCodeName.Trim().ToUpper();
            Console.WriteLine("--------");
            Console.WriteLine(code);

            var promo = await _promoRepository
                .GetQueryable()
                .FirstOrDefaultAsync(p => p.PromoCodeName == code);

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
                }
            };
        }
        catch (Exception ex)
        {
            throw new AppException("Error while verifying promo code", ex, 500);
        }
    }
}