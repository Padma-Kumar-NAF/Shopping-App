using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Repositories;

namespace ShoppingApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IUserHashRepository _userHashRepository;
        public UserService(IUserRepository userRepository, IPasswordService passwordService, IUserHashRepository userHashRepository)
        {
            _userRepository = userRepository;
            _userHashRepository = userHashRepository;
            _passwordService = passwordService;
        }
        public async Task<CreateUserResponseDTO> CreateUser(CreateUserRequestDTO request)
        {
            var (hash, salt) = await _passwordService.HashPasswordAsync(request.Password);

            string HashedPassword = Convert.ToBase64String(hash);
            string stringSalt = Convert.ToBase64String(salt);

            User User = new User();
            User.Name = request.Name;
            User.Email = request.Email;
            User.Password = HashedPassword;
            User.Role = "User";

            var AddedUser = await _userRepository.AddUser(User);
            var AddedHash = await _userHashRepository.AddHash(AddedUser.UserId, stringSalt);

            CreateUserResponseDTO response = new CreateUserResponseDTO();
            response.Password = HashedPassword;
            response.Email = request.Email;
            response.Name = request.Name;

            return response;
        }

        public async Task<LoginResponseDTO> LoginUser(LoginRequestDTO request)
        {
            // Get the UserHash
            throw new NotImplementedException();
        }
    }
}
