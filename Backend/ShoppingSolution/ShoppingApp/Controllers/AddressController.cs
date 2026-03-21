using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Address;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "user")]
    [Route("[controller]")]
    [ApiController]
    public class AddressController : BaseController
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [Authorize(Roles = "user")]
        [HttpPut("create-address")]
        [ValidateRequest]
        public async Task<IActionResult> AddAddress([FromBody] CreateNewAddressRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _addressService.AddAddress(UserId,request);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpDelete("delete-address")]
        [ValidateRequest]
        public async Task<IActionResult> DeleteUserAddress([FromBody] DeleteUserAddressRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _addressService.DeleteUserAddress(UserId,request);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost("edit-address")]
        [ValidateRequest]
        public async Task<IActionResult> EditUserAddress([FromBody] EditUserAddressRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var addressList = await _addressService.EditUserAddress(UserId,request);

                return Ok(addressList);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost("get-address")]
        [ValidateRequest]
        public async Task<IActionResult> GetUserAddress([FromBody] GetUserAddressRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var addressList = await _addressService.GetUserAddress(UserId,request);

                return Ok(addressList);
            }
            catch
            {
                throw;
            }
        }
    }
}