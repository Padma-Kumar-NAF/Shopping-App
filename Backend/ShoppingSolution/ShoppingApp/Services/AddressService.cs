using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Address;
using ShoppingApp.Models.DTOs.Order;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShoppingApp.Services
{
    public class AddressService : IAddressService
    {
        IRepository<Guid,Address> _repository;
        public AddressService(IRepository<Guid, Address> repository)
        {
            _repository = repository;
        }

        public async Task<CreateNewAddressResponseDTO> AddAddress(CreateNewAddressRequestDTO request)
        {
            var newAddress = new Address
            {
                UserId = request.UserId,
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 = request.AddressLine2.Trim(),
                State = request.State.Trim(),
                City = request.City.Trim(),
                Pincode = request.PinCode.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var address = await _repository.AddAsync(newAddress);

            if(address == null)
            {
                throw new Exception("Unable to add a address for this moment");
            }

            CreateNewAddressResponseDTO addedAddress = new CreateNewAddressResponseDTO();

            addedAddress.AddressId = address.AddressId;
            addedAddress.UserId = address.UserId;
            addedAddress.AddressLine1 = address.AddressLine1;
            addedAddress.AddressLine2 = address.AddressLine2;
            addedAddress.State = address.State;
            addedAddress.City = address.City;
            addedAddress.PinCode = addedAddress.PinCode;

            return addedAddress;
        }

        public async Task<GetUserAddressResposneDTO> GetUserAddress(GetUserAddressRequestDTO request)
        {
            if (request == null)
                return new GetUserAddressResposneDTO
                {
                    UserId = request.UserId,
                    AddressList = new List<AddressDTO>()
                };

            //if (request.PageNumber <= 0 || request.Limit <= 0)
            //    throw new ArgumentException("Invalid pagination values.");

            var query = _repository.GetQueryable()
                .Where(a => a.UserId == request.UserId);

            var addressList = await query
                .OrderBy(a => a.CreatedAt)
                .Skip((request.PageNumber - 1) * request.Limit)
                .Take(request.Limit)
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
                UserId = request.UserId,
                AddressList = addressList
            };
        }

        public async Task<bool> DeleteUserAddress(DeleteUserAddressRequestDTO request)
        {
            if (request == null || request.AddressId == Guid.Empty)
                return false;

            // Check if address exists and belongs to this user
            var address = await _repository.FirstOrDefaultAsync(a =>
                a.AddressId == request.AddressId &&
                a.UserId == request.UserId);

            if (address == null)
                return false;

            var deleted = await _repository.DeleteAsync(request.AddressId);

            return deleted != null;
        }
    }
}
