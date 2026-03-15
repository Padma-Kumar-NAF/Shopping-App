using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShoppingApp.Contexts;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Services
{
    public class UserService : IUserService
    {
        private readonly ShoppingContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IRepository<Guid,User> _userRepository;

        public UserService(ShoppingContext context, IPasswordService passwordService, IUserDetailsService userDetailsService,
            IRepository<Guid,User> userRepository)
        {
            _context = context;
            _passwordService = passwordService;
            _userRepository = userRepository;
        }

        public async Task<CreateUserResponseDTO?> CreateUser(CreateUserRequestDTO request)
        {
            var email = request.Email.Trim().ToLower();

            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
                throw new AppException("Email already exists");

            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var (hash, salt) = await _passwordService.HashPasswordAsync(request.Password);

                var user = new User
                {
                    Name = request.Name.Trim(),
                    Email = email,
                    Password = Convert.ToBase64String(hash),
                    SaltValue = Convert.ToBase64String(salt),
                    Role = "User"
                };

                await _context.Users.AddAsync(user);
                var result = await _context.SaveChangesAsync();

                if (result <= 0)
                    throw new AppException("Failed to create user.");

                var userDetails = new UserDetails
                {
                    UserId = user.UserId,
                    Name = request.Name.Trim(),
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    AddressLine1 = request.AddressLine1,
                    AddressLine2 = request.AddressLine2,
                    State = request.State,
                    City = request.City,
                    Pincode = request.Pincode
                };

                await _context.UserDetails.AddAsync(userDetails);

                var address = new Address
                {
                    UserId = user.UserId,
                    AddressLine1 = request.AddressLine1,
                    AddressLine2 = request.AddressLine2,
                    State = request.State,
                    City = request.City,
                    Pincode = request.Pincode
                };

                await _context.Addresses.AddAsync(address);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new CreateUserResponseDTO
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    UserDetails = new CreateUserDetailsDTO
                    {
                        UserId = user.UserId,
                        Name = request.Name.Trim(),
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        AddressLine1 = request.AddressLine1,
                        AddressLine2 = request.AddressLine2,
                        State = request.State,
                        City = request.City,
                        Pincode = request.Pincode
                    }
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    public async Task<LoginResponseDTO?> LoginUser(LoginRequestDTO request)
        {
            if (request == null)
                return null;

            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                throw new AppException("Email not found");

            bool isValid = await _passwordService.VerifyPasswordAsync(
                request.Password,
                user.Password,
                user.SaltValue);

            if (!isValid)
                throw new AppException("Invalid Password");

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
                    UserId = user.UserDetails!.UserDetailsId,
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

        public async Task<EditUserEmailResponseDTO> EditUserEmail(EditUserEmailRequestDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.UserId && u.Email == request.OldEmail);

            if (user == null)
                throw new AppException("User not found");

            bool isValid = await _passwordService.VerifyPasswordAsync(
                request.Password,
                user.Password,
                user.SaltValue);

            if (!isValid)
                throw new AppException("Invalid Password");

            user.Email = request.NewEmail;

            await _context.SaveChangesAsync();

            return new EditUserEmailResponseDTO()
            {
                isSuccess = true
            };
        }
    }
}