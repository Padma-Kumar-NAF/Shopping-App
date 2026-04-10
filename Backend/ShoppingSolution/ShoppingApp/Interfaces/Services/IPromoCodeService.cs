using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Promocode;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IPromoCodeService
    {
        public Task<ApiResponse<AddPromoCodeResponseDTO>> AddPromoCode(AddPromoCodeRequestDTO request);
        public Task<ApiResponse<GetAllPromocodeResponseDTO>> GetAllPromocode(GetAllPromocodeRequestDTO request);
        public Task<ApiResponse<EditPromocodeResponseDTO>> EditPromoCode(EditPromocodeRequestDTO request);
        public Task<ApiResponse<DeletePromocodeResponseDTO>> DeletePromoCode(DeletePromocodeRequestDTO request);
        //public Task<ApiResponse<VerifyPromoCodeResponseDTO>> VerifyPromoCode(VerifyPromoCodeRequestDTO request);
    }
}