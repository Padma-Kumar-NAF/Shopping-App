using Azure.Core;
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
        public async Task<CreateNewAddressResponseDTO> AddAddress(Guid UserId, CreateNewAddressRequestDTO request)
        {
            try
            {
                var user = await _userRepository.GetQueryable()
                    .FirstOrDefaultAsync(u => u.UserId == UserId);

                if (user == null)
                {
                    throw new AppException("User does not exist");
                }

                var normalizedRequest = request.AddressLine1.Trim().ToLower();
                var isExited = await _repository.GetQueryable().FirstOrDefaultAsync(a => a.UserId == UserId && a.AddressLine1.Trim().ToLower() == normalizedRequest);

                if (isExited != null)
                {
                    throw new AppException("Address already exists");
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
                    throw new AppException("Unable to add address at this moment");
                }

                return new CreateNewAddressResponseDTO
                {
                    AddressId = address.AddressId,
                };
            }
            catch (AppException)
            {
                throw;
            }
        }

        public async Task<bool> DeleteUserAddress(Guid UserId, DeleteUserAddressRequestDTO request)
        {
            try
            {
                var address = await _repository.FirstOrDefaultAsync(a => a.AddressId == request.AddressId && a.UserId == UserId);

                if (address == null)
                    throw new AppException("Address not found or does not belong to user");

                var isUsedInOrder = await _orderRepository.GetQueryable().AnyAsync(o => o.AddressId == request.AddressId);

                if (isUsedInOrder)
                    throw new AppException("Cannot delete address because it is associated with existing orders");

                var deleted = await _repository.DeleteAsync(request.AddressId);

                return deleted != null;
            }
            catch (AppException)
            {
                throw;
            }
        }

        public async Task<EditUserAddressResponseDTO> EditUserAddress(Guid UserId, EditUserAddressRequestDTO request)
        {
            try
            {
                var address = await _repository.GetQueryable()
                    .FirstOrDefaultAsync(a => a.AddressId == request.AddressId && a.UserId == UserId);

                if (address == null)
                    throw new AppException("Address not found or does not belong to user");

                var isUsedInOrder = await _orderRepository.GetQueryable()
                    .AnyAsync(o => o.AddressId == request.AddressId);

                if (isUsedInOrder)
                    throw new AppException("Cannot edit address because it is associated with existing orders");

                string normalize(string s) => (s ?? string.Empty).Trim().ToLower();

                bool isChanged =
                    normalize(address.AddressLine1) != normalize(request.AddressLine1) ||
                    normalize(address.AddressLine2) != normalize(request.AddressLine2) ||
                    normalize(address.City) != normalize(request.City) ||
                    normalize(address.State) != normalize(request.State) ||
                    normalize(address.Pincode) != normalize(request.Pincode);

                if (!isChanged)
                {
                    return new EditUserAddressResponseDTO { isSuccess = true };
                }

                var duplicate = await _repository.GetQueryable().AnyAsync(a => a.UserId == UserId && a.AddressId != request.AddressId
                                   && (a.AddressLine1).Trim().ToLower() == (request.AddressLine1).Trim().ToLower());

                if (duplicate)
                    throw new AppException("Another address with the same AddressLine1 already exists");

                address.AddressLine1 = request.AddressLine1.Trim();
                address.AddressLine2 = request.AddressLine2.Trim();
                address.City = request.City.Trim();
                address.State = request.State.Trim();
                address.Pincode = request.Pincode.Trim();

                var updated = await _repository.UpdateAsync(request.AddressId,address);

                if (updated == null)
                    throw new AppException("Unable to update address at this moment");

                return new EditUserAddressResponseDTO { isSuccess = true };
            }
            catch (AppException)
            {
                throw;
            }
        }

        public async Task<GetUserAddressResposneDTO> GetUserAddress(Guid UserId,GetUserAddressRequestDTO request)
        {
            var query = _repository.GetQueryable().Where(a => a.UserId == UserId);

            if(query == null)
            {
                return new GetUserAddressResposneDTO()
                {
                    AddressList = new List<AddressDTO>()
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
                })
                .ToListAsync();

            return new GetUserAddressResposneDTO
            {
                AddressList = addressList
            };
        }
    }
}