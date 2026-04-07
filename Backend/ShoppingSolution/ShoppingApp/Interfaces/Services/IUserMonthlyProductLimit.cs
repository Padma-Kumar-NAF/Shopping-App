using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.UserMonthlyProductLimit;

namespace ShoppingApp.Interfaces.Services
{
    public interface IUserMonthlyProductLimit
    {
        public Task<ApiResponse<AddUserMonthlyProductLimitResponseDTO>> AddLimit(AddUserMonthlyProductLimitRequestDTO request);
        public Task<ApiResponse<EditUserMonthlyProductLimitResponseDTO>> EditLimit(EditUserMonthlyProductLimitRequestDTO request);
        public Task<ApiResponse<DeleteUserMonthlyProductLimitResponseDTO>> DeleteLimit(DeleteUserMonthlyProductLimitRequestDTO request);
        public Task<ApiResponse<GetAllUserMonthlyProductLimitResponseDTO>> GetAllLimits(GetAllUserMonthlyProductLimitRequestDTO request);
    }
}
