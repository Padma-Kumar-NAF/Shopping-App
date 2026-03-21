using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Address;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IAddressController
    {
        public Task<IActionResult> AddAddress(CreateNewAddressRequestDTO request);
        public Task<IActionResult> GetUserAddress(GetUserAddressRequestDTO request);
        public Task<IActionResult> DeleteUserAddress(DeleteUserAddressRequestDTO request);
        public Task<IActionResult> EditUserAddress(EditUserAddressRequestDTO request);
    }
}
