using Microsoft.EntityFrameworkCore;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.Services;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Promocode;
using ShoppingApp.Models.DTOs.UserPromoCodes;
using ShoppingApp.Models.Entities;

namespace ShoppingApp.Services
{
    public class UserPromoCodeService : IUserPromoCodeService
    {
        private readonly IRepository<Guid, UserPromoCode> _userPromoCodeRepository;
        public UserPromoCodeService(IRepository<Guid, UserPromoCode> userPromoCodeRepository) {
            _userPromoCodeRepository = userPromoCodeRepository;
        }

        public async Task<ApiResponse<GetAllUserPromoCodesResponseDTO>> GetAllUserPromoCodes(Guid userId)
        {
            var promoCodes = await _userPromoCodeRepository.GetQueryable()
                .AsNoTracking()
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Select(p => new UserPromoCodeItemDTO
                {
                    PromoCodeId = p.UserPromoCodeId,
                    PromoCodeName = p.PromoCodeName,
                    DiscountPercentage = p.DiscountPercentage,
                    FromDate = p.FromDate,
                    ToDate = p.ToDate
                })
                .ToListAsync();

            promoCodes ??= new List<UserPromoCodeItemDTO>();

            return new ApiResponse<GetAllUserPromoCodesResponseDTO>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = new GetAllUserPromoCodesResponseDTO
                {
                    PromoCodes = promoCodes,
                    TotalCount = promoCodes.Count
                },
                Message = promoCodes.Count == 0
                    ? "No promo codes found"
                    : "Promo codes retrieved successfully",
                Action = "ShowPromoCode"
            };
        }

        public async Task<ApiResponse<VerifyPromoCodeResponseDTO>> VerifyPromoCode(VerifyPromoCodeRequestDTO request)
        {
            var code = request.PromoCodeName.Trim().ToUpper();

            var promo = await _userPromoCodeRepository.GetQueryable().FirstOrDefaultAsync(p => p.PromoCodeName == code && !p.IsDeleted);

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
                    PromoCodeId = promo.UserPromoCodeId,
                    Message = "Promo code applied successfully"
                },
                Message = "Promo code applied successfully"
            };
        }
    }
}
