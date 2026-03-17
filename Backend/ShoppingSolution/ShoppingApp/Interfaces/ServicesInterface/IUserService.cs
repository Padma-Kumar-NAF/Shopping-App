using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IUserService
    {
        public Task<CreateUserResponseDTO?> CreateUser(CreateUserRequestDTO request);
        public Task<LoginResponseDTO?> LoginUser(LoginRequestDTO request);
        public Task<EditUserEmailResponseDTO> EditUserEmail(EditUserEmailRequestDTO request);
        public Task<IEnumerable<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request);
        public Task<CreateUserResponseDTO> GetUserById(Guid UserId);
    }
}
