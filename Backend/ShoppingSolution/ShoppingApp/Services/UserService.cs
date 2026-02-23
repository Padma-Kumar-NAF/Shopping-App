using FirstAPI.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;
using ShoppingApp.Repositories;

namespace ShoppingApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        public UserService(IUserRepository userRepository, IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
        }
        public async Task<CreateUserResponseDTO> CreateUser(CreateUserRequestDTO request)
        {
            var email = await _userRepository.GetUserByMail(request.Email);
            if(email != null)
            {
                throw new UnAuthorizedException("Email already exists");
            }
            var (hash, salt) = await _passwordService.HashPasswordAsync(request.Password);

            string HashedPassword = Convert.ToBase64String(hash);

            User User = new User();
            User.Name = request.Name;
            User.Email = request.Email;
            User.Password = HashedPassword;
            User.SaltValue = Convert.ToBase64String(salt);
            User.Role = "User";

            var AddedUser = await _userRepository.AddUser(User);

            CreateUserResponseDTO response = new CreateUserResponseDTO();
            response.Password = HashedPassword;
            response.Email = request.Email;
            response.Name = request.Name;

            return response;
        }

        public async Task<LoginResponseDTO> LoginUser(LoginRequestDTO request)
        {
            var user = await _userRepository.GetUserByMail(request.Email);
            if (user == null)
            {
                throw new UnAuthorizedException("Invalid Email");
            }
            bool isValidUser = await _passwordService.VerifyPasswordAsync(request.Password,user.Password, user.SaltValue);
            if (!isValidUser)
            {
                throw new UnAuthorizedException("Invalid Password");
            }
            LoginResponseDTO response = new LoginResponseDTO();
            response.Email = request.Email;
            response.Password = request.Password;
            response.Role = user.Role;
            Console.WriteLine("----------------------------");
            Console.WriteLine(user.UserId);
            Console.WriteLine(user.Name);
            Console.WriteLine(user.Password);
            Console.WriteLine(user.Role);
            Console.WriteLine(user.SaltValue);
            Console.WriteLine("----------------------------");
            return response;
        }

        public async Task<IEnumerable<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request)
        {
            var UsersList = await _userRepository.GetUsers(request.Limit , request.PageNumber);
            if(UsersList == null || !UsersList.Any())
            {
                throw new Exception("No Users Found");
            }
            return UsersList;
        }
    }
}