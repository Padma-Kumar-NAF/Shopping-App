using FirstAPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;
using ShoppingApp.Repositories;

namespace ShoppingApp.Services
{
    public class UserService : IUserService
    {
        private readonly ShoppingContext _context;
        private readonly IPasswordService _passwordService;

        public UserService(ShoppingContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<CreateUserResponseDTO> CreateUser(CreateUserRequestDTO request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                throw new UnAuthorizedException("Email already exists");

            var (hash, salt) = await _passwordService.HashPasswordAsync(request.Password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Password = Convert.ToBase64String(hash),
                SaltValue = Convert.ToBase64String(salt),
                Role = "User"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new CreateUserResponseDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Password = user.Password
            };
        }

        public async Task<LoginResponseDTO> LoginUser(LoginRequestDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnAuthorizedException("Invalid Email");

            bool isValid = await _passwordService.VerifyPasswordAsync(
                request.Password,
                user.Password,
                user.SaltValue);

            if (!isValid)
                throw new UnAuthorizedException("Invalid Password");

            return new LoginResponseDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<IEnumerable<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request)
        {
            if (request.PageNumber <= 0) request.PageNumber = 1;
            if (request.Limit <= 0) request.Limit = 10;

            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserDetails)
                .OrderBy(u => u.Name)
                .Skip((request.PageNumber - 1) * request.Limit)
                .Take(request.Limit)
                .Select(u => new GetUsersResponseDTO
                {
                    UserId = u.UserId,
                    UserDetailsId = u.UserDetails!.UserDetailsId,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    PhoneNumber = u.UserDetails.PhoneNumber,
                    AddressLine1 = u.UserDetails.AddressLine1,
                    AddressLine2 = u.UserDetails.AddressLine2,
                    State = u.UserDetails.State,
                    City = u.UserDetails.City,
                    Pincode = u.UserDetails.Pincode
                })
                .ToListAsync();

            if (!users.Any())
                throw new Exception("No Users Found");

            return users;
        }
    }
}