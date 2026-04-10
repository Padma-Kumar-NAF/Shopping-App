using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Address;
using ShoppingApp.Models.DTOs.Order;

namespace ShoppingApp.Services
{
    public class AddressService : IAddressService
    {
        IRepository<Guid,Address> _repository;
        IRepository<Guid,User> _userRepository;
        IRepository<Guid, Order> _orderRepository;
        public AddressService(IRepository<Guid, Address> repository, IRepository<Guid, User> userRepository, IRepository<Guid, Order> orderRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
        }
        public async Task<ApiResponse<CreateNewAddressResponseDTO>> AddAddress(Guid UserId, CreateNewAddressRequestDTO request)
        {
            var user = await _userRepository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(u => u.UserId == UserId);

            if (user == null)
            {
                throw new AppException("User does not exist", 404);
            }

            var normalizedRequest = request.AddressLine1.Trim().ToLower();

            var isExited = await _repository.GetQueryable().AnyAsync(a => a.UserId == UserId && a.AddressLine1.Trim().ToLower() == normalizedRequest);

            if (isExited)
            {
                throw new AppException("Address already exists", 409);
            }

            var newAddress = new Address
            {
                UserId = UserId,
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 = request.AddressLine2.Trim(),
                State = request.State.Trim(),
                City = request.City.Trim(),
                Pincode = request.PinCode.Trim()
            };

            var address = await _repository.AddAsync(newAddress);

            if (address == null)
            {
                throw new AppException("Unable to add address at this moment", 500);
            }

            return new ApiResponse<CreateNewAddressResponseDTO>()
            {
                StatusCode = 200,
                Message = "Address added successfully",
                Data = new CreateNewAddressResponseDTO
                {
                    AddressId = address.AddressId,
                },
                Action = "Add Address"
            };
        }

        public async Task<ApiResponse<DeleteUserAddressResponseDTO>> DeleteUserAddress(Guid UserId, DeleteUserAddressRequestDTO request)
        {
            var address = await _repository.FirstOrDefaultAsync(a => a.AddressId == request.AddressId && a.UserId == UserId);

            if (address == null)
                throw new AppException("Address not found or does not belong to user", 404);

            var isUsedInOrder = await _orderRepository.GetQueryable().AnyAsync(o => o.AddressId == request.AddressId);

            if (isUsedInOrder)
                throw new AppException("Cannot delete address because it is associated with existing orders", 409);

            var deleted = await _repository.DeleteAsync(request.AddressId);
            if (deleted == null)
            {
                throw new AppException("Failed to delete address", 500);
            }
            return new ApiResponse<DeleteUserAddressResponseDTO>()
            {
                StatusCode = 200,
                Data = new DeleteUserAddressResponseDTO()
                {
                    IsSuccess = true,
                },
                Action = "DeleteAddress",
                Message = "Address deleted successfully"
            };
        }

        public async Task<ApiResponse<EditUserAddressResponseDTO>> EditUserAddress(Guid UserId, EditUserAddressRequestDTO request)
        {
            var address = await _repository.GetQueryable().FirstOrDefaultAsync(a => a.AddressId == request.AddressId && a.UserId == UserId);

            if (address == null)
            {
                throw new AppException("Address not found or does not belong to user", 404);
            }

            var isUsedInOrder = await _orderRepository.GetQueryable().AnyAsync(o => o.AddressId == request.AddressId);

            if (isUsedInOrder)
            {
                throw new AppException("Cannot edit address because it is associated with existing orders", 409);
            }

            string normalize(string s) => (s ?? string.Empty).Trim().ToLower();

            bool isChanged =
                normalize(address.AddressLine1) != normalize(request.AddressLine1) ||
                normalize(address.AddressLine2) != normalize(request.AddressLine2) ||
                normalize(address.City) != normalize(request.City) ||
                normalize(address.State) != normalize(request.State) ||
                normalize(address.Pincode) != normalize(request.Pincode);

            if (!isChanged)
            {
                return new ApiResponse<EditUserAddressResponseDTO>()
                {
                    Data = new EditUserAddressResponseDTO { IsSuccess = true },
                    Action = "NoChangesRequired",
                    Message = "Address updated",
                    StatusCode = 200
                };
            }

            var duplicate = await _repository.GetQueryable().AnyAsync(a => a.UserId == UserId && a.AddressId != request.AddressId
                                && (a.AddressLine1).Trim().ToLower() == (request.AddressLine1).Trim().ToLower());

            if (duplicate)
            {
                throw new AppException("Another address with the same AddressLine1 already exists", 409);
            }

            address.AddressLine1 = request.AddressLine1.Trim();
            address.AddressLine2 = request.AddressLine2.Trim();
            address.City = request.City.Trim();
            address.State = request.State.Trim();
            address.Pincode = request.Pincode.Trim();


            await _repository.UpdateAsync(request.AddressId, address);
            return new ApiResponse<EditUserAddressResponseDTO>()
            {
                Data = new EditUserAddressResponseDTO { IsSuccess = true },
                Action = "NoChangesRequired",
                Message = "Address updated",
                StatusCode = 200
            };
        }

        public async Task<ApiResponse<GetUserAddressResposneDTO>> GetUserAddress(Guid UserId,GetUserAddressRequestDTO request)
        {
            var query = _repository.GetQueryable().Where(a => a.UserId == UserId);

            if (!await query.AnyAsync())
            {
                return new ApiResponse<GetUserAddressResposneDTO>()
                {
                    Data = new GetUserAddressResposneDTO()
                    {
                        AddressList = new List<AddressDTO>()
                    },
                    Message = "No address found",
                    Action = "Show address button",
                    StatusCode = 200
                };
            }

            var addressList = await query
                .OrderBy(a => a.CreatedAt)
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(a => new AddressDTO
                {
                    AddressId = a.AddressId,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    State = a.State,
                    City = a.City,
                    Pincode = a.Pincode
                }).ToListAsync();

            return new ApiResponse<GetUserAddressResposneDTO>()
            {
                Data = new GetUserAddressResposneDTO
                {
                    AddressList = addressList
                },
                StatusCode = 200,
                Action = "Show address list",
                Message = "Address fetched successfully"
            };
        }
    }
}