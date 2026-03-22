using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IUserService
    {
        public Task<ApiResponse<CreateUserResponseDTO>> CreateUser(CreateUserRequestDTO request);
        public Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginRequestDTO request);
        public Task<ApiResponse<EditUserEmailResponseDTO>> EditUserEmail(Guid userId,EditUserEmailRequestDTO request);
        public Task<ApiResponse<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request);
        public Task<ApiResponse<GetUserByIdResponseDTO>> GetUserById(Guid UserId);
        public Task<ApiResponse<UpdateProfileResponseDTO>> UpdateUserDetails(Guid userId,UpdateProfileRequestDTO request);
    }
}
