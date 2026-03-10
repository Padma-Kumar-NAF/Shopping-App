using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
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
            try
            {
                var newAddress = new Address
                {
                    UserId = request.UserId,
                    AddressLine1 = request.AddressLine1.Trim(),
                    AddressLine2 = request.AddressLine2.Trim(),
                    State = request.State.Trim(),
                    City = request.City.Trim(),
                    Pincode = request.PinCode.Trim()
                };

                var address = await _repository.AddAsync(newAddress);

                if (address != null)
                {
                    throw new AppException("Unable to add a address for this moment");
                }

                return new CreateNewAddressResponseDTO()
                {
                    AddressId = address.AddressId,
                    UserId = address.UserId,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    State = address.State,
                    City = address.City,
                    PinCode = address.Pincode
                };
            }
            catch(Exception ex)
            {
                throw new AppException("Unable to create address",ex);
            }
        }

        public async Task<bool> DeleteUserAddress(DeleteUserAddressRequestDTO request)
        {   
            var address = await _repository.FirstOrDefaultAsync(a =>
                a.AddressId == request.AddressId &&
                a.UserId == request.UserId);

            if (address == null)
                throw new AppException($"Address not found or does not belong to user");

            var deleted = await _repository.DeleteAsync(request.AddressId);

            return deleted != null;
        }

        public async Task<GetUserAddressResposneDTO> GetUserAddress(GetUserAddressRequestDTO request)
        { 
            var query = _repository.GetQueryable()
                .Where(a => a.UserId == request.UserId);

            if(query == null)
            {
                return new GetUserAddressResposneDTO()
                {
                    UserId = request.UserId,
                    AddressList = new List<AddressDTO>()
                };
            }

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
    }
}
