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
        public async Task<ApiResponse<CreateUserResponseDTO>> CreateUser(CreateUserRequestDTO request)
        {
            var email = request.Email.Trim().ToLower();

            var existingUser = await _userRepository.GetQueryable()
                .AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                throw new AppException("Email already exists",409);
            }

            var existingPhoneNumber = await _userDetailsRepository.GetQueryable().AsNoTracking()
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if(existingPhoneNumber != null)
            {
                throw new AppException("Phone number already exists",409);
            }

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
                    Role = "user"
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

                return new ApiResponse<CreateUserResponseDTO>()
                {
                    Data = new CreateUserResponseDTO
                    {
                        IsSuccess = true,
                    },
                    StatusCode = 201,
                    Action = "AddUserDetails",
                    Message = "User created successfully"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Server error occurred while creating user", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Server error occurred while creating user", ex, 500);
            }
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginRequestDTO request)
        {
            try
            {
                var email = request.Email.Trim().ToLower();

                if (email == null)
                {
                    throw new AppException("Email not found",404);
                }

                var user = await _userRepository.GetQueryable()
                    .FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    throw new AppException("Invalid email", 400);
                }

                bool isValid = await _passwordService.VerifyPasswordAsync(request.Password,user.Password,user.SaltValue);

                if (!isValid)
                {
                    throw new AppException("Invalid Password",400);
                }

                LoginResponseDTO response = new LoginResponseDTO()
                {
                    Token = _tokenService.GenerateToken(user.UserId,user.Name,user.Email,user.Role)
                };

                return new ApiResponse<LoginResponseDTO>()
                {
                    Data = response,
                    Message = "Logged in successfully",
                    Action = "MoveToHome",
                    StatusCode = 200
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while login", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while login", ex, 500);
            }
        }

        public async Task<ApiResponse<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request)
        {
            try
            {
                var pageNumber = request.Pagination.PageNumber <= 0 ? 1 : request.Pagination.PageNumber;
                var pageSize = request.Pagination.PageSize <= 0 ? 10 : request.Pagination.PageSize;

                var query = _userRepository.GetQueryable().AsNoTracking().Include(u => u.UserDetails);

                var totalCount = await query.CountAsync();

                if (totalCount == 0)
                {
                    return new ApiResponse<GetUsersResponseDTO>()
                    {
                        Data = new GetUsersResponseDTO(),
                        StatusCode = 200,
                        Message = "No users found",
                        Action = "ShowEmptyPage"
                    };
                }

                var users = await query
                    .OrderBy(u => u.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserDetailsDTO
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

                var response = new GetUsersResponseDTO
                {
                    UsersList = users
                };

                return new ApiResponse<GetUsersResponseDTO>()
                {
                    Data = response,
                    StatusCode = 200,
                    Message = "Users fetched successfully"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while fetching users", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while fetching users", ex, 500);
            }
        }

        public async Task<ApiResponse<GetUserByIdResponseDTO>> GetUserById(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetQueryable().AsNoTracking().Include(u => u.UserDetails)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    throw new AppException("User not found", 404);
                }

                var response = new GetUserByIdResponseDTO
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    UserDetails = new GetUserDetailsDTO
                    {
                        UserDetailsId = user.UserDetails!.UserDetailsId,
                        PhoneNumber = user.UserDetails.PhoneNumber,
                        AddressLine1 = user.UserDetails.AddressLine1,
                        AddressLine2 = user.UserDetails.AddressLine2,
                        State = user.UserDetails.State,
                        City = user.UserDetails.City,
                        Pincode = user.UserDetails.Pincode
                    }
                };

                return new ApiResponse<GetUserByIdResponseDTO>()
                {
                    Data = response,
                    StatusCode = 200,
                    Message = "User fetched successfully",
                    Action = "ShowUser"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while fetching user", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while fetching user", ex, 500);
            }
        }

        public async Task<ApiResponse<EditUserEmailResponseDTO>> EditUserEmail(Guid userId, EditUserEmailRequestDTO request)
        {
            try
            {
                var user = await _userRepository.GetQueryable()
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.Email == request.OldEmail);

                if (user == null)
                {
                    throw new AppException("User not found", 404);
                }

                var existingEmail = await _userRepository.GetQueryable()
                    .FirstOrDefaultAsync(u => u.Email == request.NewEmail);

                if (existingEmail != null)
                {
                    throw new AppException("Email already exists", 400);
                }

                bool isValid = await _passwordService.VerifyPasswordAsync(request.Password,user.Password,user.SaltValue);

                if (!isValid)
                {
                    throw new AppException("Invalid password", 401);
                }

                await _unitOfWork.BeginTransactionAsync();

                user.Email = request.NewEmail;

                await _userRepository.UpdateAsync(user.UserId, user);

                var userDetails = await _userDetailsRepository.GetQueryable().FirstOrDefaultAsync(ud => ud.UserId == userId);

                if (userDetails == null)
                {
                    throw new AppException("User details not found", 404);
                }

                userDetails.Email = request.NewEmail;
                await _userDetailsRepository.UpdateAsync(userDetails.UserDetailsId, userDetails);

                await _unitOfWork.CommitAsync();

                return new ApiResponse<EditUserEmailResponseDTO>()
                {
                    Data = new EditUserEmailResponseDTO
                    {
                        IsSuccess = true
                    },
                    StatusCode = 200,
                    Message = "Email updated successfully"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Error while updating email", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while updating email", ex, 500);
            }
        }

        public async Task<ApiResponse<UpdateProfileResponseDTO>> UpdateUserDetails(Guid userId, UpdateProfileRequestDTO request)
        {
            try
            {
                var user = await _userRepository.GetQueryable()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    throw new AppException("User not found", 404);
                }

                var userDetails = await _userDetailsRepository.GetQueryable()
                    .FirstOrDefaultAsync(ud => ud.UserId == userId);

                if (userDetails == null)
                {
                    throw new AppException("User details not found", 404);
                }

                await _unitOfWork.BeginTransactionAsync();

                if (user.Name != request.Details.Name)
                {
                    user.Name = request.Details.Name;
                    await _userRepository.UpdateAsync(user.UserId, user);
                }

                userDetails.Name = request.Details.Name;
                userDetails.PhoneNumber = request.Details.PhoneNumber;
                userDetails.AddressLine1 = request.Details.AddressLine1;
                userDetails.AddressLine2 = request.Details.AddressLine2;
                userDetails.State = request.Details.State;
                userDetails.City = request.Details.City;
                userDetails.Pincode = request.Details.Pincode;

                await _userDetailsRepository.UpdateAsync(userDetails.UserDetailsId, userDetails);

                var address = await _addressRepository.GetQueryable().FirstOrDefaultAsync(a => a.UserId == userId);

                if (address != null)
                {
                    address.AddressLine1 = request.Details.AddressLine1;
                    address.AddressLine2 = request.Details.AddressLine2;
                    address.State = request.Details.State;
                    address.City = request.Details.City;
                    address.Pincode = request.Details.Pincode;

                    await _addressRepository.UpdateAsync(address.AddressId, address);
                }
                else
                {
                    address = new Address
                    {
                        UserId = userId,
                        AddressLine1 = request.Details.AddressLine1,
                        AddressLine2 = request.Details.AddressLine2,
                        State = request.Details.State,
                        City = request.Details.City,
                        Pincode = request.Details.Pincode
                    };

                    await _addressRepository.AddAsync(address);
                }

                await _unitOfWork.CommitAsync();
                
                return new ApiResponse<UpdateProfileResponseDTO>()
                {
                    Data = new UpdateProfileResponseDTO
                    {
                        IsSuccess = true
                    },
                    StatusCode = 200,
                    Message = "Profile updated successfully"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Error while updating profile", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Something went wrong while updating profile", ex, 500);
            }
        }
    }
}