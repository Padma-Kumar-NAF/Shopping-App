using Microsoft.EntityFrameworkCore;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Services
{
    public class UserDetailsService : IUserDetailsService
    {
        private readonly IRepository<Guid, UserDetails> _userDetailsRepository;
        private readonly IRepository<Guid, User> _userRepository;
        private readonly IRepository<Guid, Address> _addressRepository;

        public UserDetailsService(
            IRepository<Guid, UserDetails> userDetailsRepository,
            IRepository<Guid, User> userRepository,
            IRepository<Guid, Address> addressRepository)
        {
            _userDetailsRepository = userDetailsRepository;
            _userRepository = userRepository;
            _addressRepository = addressRepository;
        }

        public async Task<Guid> AddUserDetails(AddUserDetailsRequestDTO request)
        {
            var user = await _userRepository.GetAsync(request.UserId);

            if (user == null)
                throw new Exception("User not found");

            var details = new UserDetails
            {
                UserId = request.UserId,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = request.PhoneNumber,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                State = request.State,
                City = request.City,
                Pincode = request.Pincode
            };

            var result = await _userDetailsRepository.AddAsync(details);

            if (result == null)
                throw new Exception("Unable to add user details");

            var address = new Address
            {
                UserId = request.UserId,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                State = request.State,
                City = request.City,
                Pincode = request.Pincode,
            };

            var addedAddress = await _addressRepository.AddAsync(address);

            if (addedAddress == null)
                throw new Exception("Unable to add address");

            return details.UserDetailsId;
        }

        public async Task<UpdateProfileResponseDTO> UpdateUserDetails(UpdateProfileRequestDTO request)
        {
            var user = await _userRepository.GetQueryable()
                .FirstOrDefaultAsync(u => u.UserId == request.UserId);

            if (user == null)
                throw new Exception("User not found");

            var userDetails = await _userDetailsRepository.GetQueryable()
                .FirstOrDefaultAsync(ud => ud.UserId == request.UserId);

            if (userDetails == null)
                throw new Exception("User details not found");

            var address = await _addressRepository.GetQueryable()
                .FirstOrDefaultAsync(a => a.UserId == request.UserId);

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
                    AddressId = Guid.NewGuid(),
                    UserId = request.UserId,
                    AddressLine1 = request.Details.AddressLine1,
                    AddressLine2 = request.Details.AddressLine2,
                    State = request.Details.State,
                    City = request.Details.City,
                    Pincode = request.Details.Pincode
                };

                await _addressRepository.AddAsync(address);
            }

            return new UpdateProfileResponseDTO
            {
                UserDetailsId = userDetails.UserDetailsId,
                Name = userDetails.Name,
                Email = user.Email,
                PhoneNumber = userDetails.PhoneNumber,
                AddressLine1 = userDetails.AddressLine1,
                AddressLine2 = userDetails.AddressLine2,
                State = userDetails.State,
                City = userDetails.City,
                Pincode = userDetails.Pincode
            };
        }
    }
}