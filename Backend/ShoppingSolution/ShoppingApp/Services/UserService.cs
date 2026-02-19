using ShoppingApp.Interfaces.Repositories;
using ShoppingApp.Interfaces.Services;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<CreateUserResponseDTO> CreateUser(CreateUserRequestDTO request)
        {
            // Do the pass hash
            User User = new User();
            User.Name = request.Name;
            User.Email = request.Email;
            User.Password = request.Password;
            User.Role = "User";

            var AddedUser = await _userRepository.AddUser(User);

            CreateUserResponseDTO response = new CreateUserResponseDTO();
            response.Password = request.Password;
            response.Email = request.Email;
            response.Name = request.Name;

            return response;
        }

        public async Task<LoginResponseDTO> LoginUser(LoginRequestDTO request)
        {
            throw new NotImplementedException();
        }
    }
}
