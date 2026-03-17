using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Services
{
    public class UserService : IUserService
    {
        private readonly IPasswordService _passwordService;
        private readonly IRepository<Guid, User> _userRepository;
        private readonly IRepository<Guid, UserDetails> _userDetailsRepository;
        private readonly IRepository<Guid, Address> _addressRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ITokenService _tokenService;

        public UserService(
            IPasswordService passwordService,
            IRepository<Guid, User> userRepository,
            IRepository<Guid, UserDetails> userDetailsRepository,
            IRepository<Guid, Address> addressRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService)
            {
                _passwordService = passwordService;
                _userRepository = userRepository;
                _userDetailsRepository = userDetailsRepository;
                _addressRepository = addressRepository;
                _unitOfWork = unitOfWork;
                _tokenService = tokenService;
            }
        public async Task<CreateUserResponseDTO> CreateUser(CreateUserRequestDTO request)
        {
            var email = request.Email.Trim().ToLower();

            var existingUser = await _userRepository.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
                throw new AppException("Email already exists");

            await _unitOfWork.BeginTransactionAsync();

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

                await _userRepository.AddAsync(user);

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

                await _userDetailsRepository.AddAsync(userDetails);

                var address = new Address
                {
                    UserId = user.UserId,
                    AddressLine1 = request.AddressLine1,
                    AddressLine2 = request.AddressLine2,
                    State = request.State,
                    City = request.City,
                    Pincode = request.Pincode
                };

                await _addressRepository.AddAsync(address);

                var result = await _unitOfWork.CommitAsync();

                if (result <= 0)
                    throw new AppException("Failed to create user. Please try again.");

                return new CreateUserResponseDTO
                {
                    isSuccess = true,
                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Server error occurred while creating user");
            }
        }

        public async Task<LoginResponseDTO> LoginUser(LoginRequestDTO request)
        {
            try
            {
                var email = request.Email.Trim().ToLower();
                var user = await _userRepository.GetQueryable()
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    throw new AppException("Email not found");

                bool isValid = await _passwordService.VerifyPasswordAsync(
                    request.Password,
                    user.Password,
                    user.SaltValue);

                if (!isValid)
                    throw new AppException("Invalid Password");

                LoginResponseDTO response = new LoginResponseDTO()
                {
                    Token = _tokenService.GenerateToken(
                    user.UserId,
                    user.Name,
                    user.Email,
                    user.Role)
                };

                return response;
            }
            catch (AppException)
            {
                throw;
            }
        }

        public async Task<IEnumerable<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request)
        {
            if (request.PageNumber <= 0) request.PageNumber = 1;
            if (request.Limit <= 0) request.Limit = 10;

            var users = await _userRepository.GetQueryable()
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

        public async Task<GetUserByIdResponseDTO> GetUserById(Guid UserId)
        {
            var user = await _userRepository.GetQueryable()
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.UserId == UserId);

            if (user == null)
                throw new AppException("No user found");

            return new GetUserByIdResponseDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                UserDetails = new CreateUserDetailsDTO
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
            var user = await _userRepository.GetQueryable()
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
            await _userRepository.UpdateAsync(user.UserId, user);

            var userDetails = await _userDetailsRepository.GetQueryable()
                .FirstOrDefaultAsync(ud => ud.UserId == request.UserId);

            userDetails!.Email = request.NewEmail;
            await _userDetailsRepository.UpdateAsync(userDetails.UserDetailsId, userDetails);

            return new EditUserEmailResponseDTO
            {
                isSuccess = true
            };
        }
    }
}