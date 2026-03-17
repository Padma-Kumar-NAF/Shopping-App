using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Address;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IAddressController
    {
        public Task<ActionResult<CreateNewAddressResponseDTO>> AddAddress(CreateNewAddressRequestDTO request);
        public Task<ActionResult<GetUserAddressResposneDTO>> GetUserAddress(GetUserAddressRequestDTO request);
        public Task<ActionResult<DeleteUserAddressResponseDTO>> DeleteUserAddress(DeleteUserAddressRequestDTO request);
        public Task<ActionResult<EditUserAddressResponseDTO>> EditUserAddress(EditUserAddressRequestDTO request);
    }
}
