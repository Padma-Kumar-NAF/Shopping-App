using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Promocode;
using ShoppingApp.Models.DTOs.UserPromoCodes;

namespace ShoppingApp.Interfaces.Services
{
    public interface IUserPromoCodeService
    {
        public Task<ApiResponse<VerifyPromoCodeResponseDTO>> VerifyPromoCode(VerifyPromoCodeRequestDTO request);
        public Task<ApiResponse<GetAllUserPromoCodesResponseDTO>> GetAllUserPromoCodes(Guid userId);
    }
}
