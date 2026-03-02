using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IUserDetailsService
    {
        public Task<Guid> AddUserDetails(AddUserDetailsRequestDTO request);
        public Task<UpdateProfileResponseDTO> UpdateUserDetails(UpdateProfileRequestDTO request);
    }
}
