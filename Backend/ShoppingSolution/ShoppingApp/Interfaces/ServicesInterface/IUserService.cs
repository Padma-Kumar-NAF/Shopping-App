using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IUserService
    {
        public Task<CreateUserResponseDTO?> CreateUser(CreateUserRequestDTO request);
        public Task<LoginResponseDTO> LoginUser(LoginRequestDTO request);
    }
}
