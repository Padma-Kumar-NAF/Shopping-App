using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Address;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IAddressService
    {
        public Task<CreateNewAddressResponseDTO> AddAddress(CreateNewAddressRequestDTO request);
        public Task<GetUserAddressResposneDTO> GetUserAddress(GetUserAddressRequestDTO request);
    }
}
