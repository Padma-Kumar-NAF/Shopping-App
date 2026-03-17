using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Address;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IAddressService
    {
        public Task<CreateNewAddressResponseDTO> AddAddress(Guid UserId ,CreateNewAddressRequestDTO request);
        public Task<GetUserAddressResposneDTO> GetUserAddress(Guid UserId,GetUserAddressRequestDTO request);
        public Task<bool> DeleteUserAddress(Guid UserId,DeleteUserAddressRequestDTO request);
        public Task<EditUserAddressResponseDTO> EditUserAddress(Guid UserId, EditUserAddressRequestDTO request);
    }
}