using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Address;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IAddressService
    {
        public Task<ApiResponse<CreateNewAddressResponseDTO>> AddAddress(Guid UserId ,CreateNewAddressRequestDTO request);
        public Task<ApiResponse<GetUserAddressResposneDTO>> GetUserAddress(Guid UserId,GetUserAddressRequestDTO request);
        public Task<ApiResponse<DeleteUserAddressResponseDTO>> DeleteUserAddress(Guid UserId,DeleteUserAddressRequestDTO request);
        public Task<ApiResponse<EditUserAddressResponseDTO>> EditUserAddress(Guid UserId, EditUserAddressRequestDTO request);
    }
}