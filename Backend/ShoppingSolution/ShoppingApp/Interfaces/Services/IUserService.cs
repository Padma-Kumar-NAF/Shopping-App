using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.Services
{
    public interface IUserService
    {
        public Task<CreateUserResponseDTO> CreateUser(CreateUserRequestDTO request);
        //public Task<LoginResponseDTO> LoginUser(LoginRequestDTO request);
    }
}
