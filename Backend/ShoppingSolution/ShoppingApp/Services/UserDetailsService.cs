using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;
using ShoppingApp.Repositories;

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
            {
                throw new Exception("User not found");
            }

            UserDetails details = new UserDetails
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

            Address address = new Address
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
    }
}
