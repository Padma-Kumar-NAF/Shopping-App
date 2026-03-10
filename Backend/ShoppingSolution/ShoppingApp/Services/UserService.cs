using FirstAPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
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

        public async Task<CreateUserResponseDTO?> CreateUser(CreateUserRequestDTO request)
        {
            var email = request.Email.Trim().ToLower();

            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
                return null;

            var (hash, salt) = await _passwordService.HashPasswordAsync(request.Password);

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = email,
                Password = Convert.ToBase64String(hash),
                SaltValue = Convert.ToBase64String(salt),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            var result = await _context.SaveChangesAsync();

            if (result <= 0)
                return null;

            return new CreateUserResponseDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                UserDetails = new CreateUserDetailsDTO()
            };
        }

        public async Task<LoginResponseDTO?> LoginUser(LoginRequestDTO request)
        {
            if (request == null)
                return null;

            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                throw new Exception("Email not found");

            bool isValid = await _passwordService.VerifyPasswordAsync(
                request.Password,
                user.Password,
                user.SaltValue);

            //Console.WriteLine("Verify");
            //Console.WriteLine(isValid);
            //Console.WriteLine("-----------------");
            //Console.WriteLine(user.UserId);

            if (!isValid)
                throw new AppException("Invalid Credentials");

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

        public async Task<CreateUserResponseDTO> GetUserById(Guid UserId)
        {
            var user = await _context.Users
               .Include(u => u.UserDetails)
               .FirstOrDefaultAsync(u => u.UserId == UserId);

            if (user == null)
            {
                throw new AppException("No user found");
            }

            return new CreateUserResponseDTO()
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                UserDetails = new CreateUserDetailsDTO()
                {
                    UserId = user.UserDetails.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.UserDetails.PhoneNumber,
                    AddressLine1 = user.UserDetails.AddressLine1,
                    AddressLine2 = user.UserDetails.AddressLine2,
                    State = user.UserDetails.State,
                    City = user.UserDetails.City,
                    Pincode = user.UserDetails.Pincode

                }
            };
        }
    }
}