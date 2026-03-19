using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Address;
using System.Security.Claims;

namespace ShoppingApp.Controllers
{
    //[Authorize(Roles = "User")]
    [Route("[controller]")]
    [ApiController]
    public class AddressController : BaseController , IAddressController
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpPut("create-address")]
        [ValidateRequest]
        public async Task<ActionResult<CreateNewAddressResponseDTO>> AddAddress(CreateNewAddressRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                {
                    return BadRequest("User not found");
                }

                var result = await _addressService.AddAddress(UserId,request);

                if (result == null)
                    return BadRequest("Unable to create address at the moment");

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("delete-address")]
        [ValidateRequest]
        public async Task<ActionResult<DeleteUserAddressResponseDTO>> DeleteUserAddress(DeleteUserAddressRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();

                if (UserId == Guid.Empty)
                    return Unauthorized("User not authenticated");

                var result = await _addressService.DeleteUserAddress(UserId,request);

                return Ok(new DeleteUserAddressResponseDTO()
                {
                    Success = result,
                });
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("edit-address")]
        [ValidateRequest]
        public async Task<ActionResult<EditUserAddressResponseDTO>> EditUserAddress(EditUserAddressRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();
                if (UserId == Guid.Empty)
                {
                    return BadRequest("User not authenticated");
                }
                var addressList = await _addressService.EditUserAddress(UserId,request);

                return Ok(addressList);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("get-address")]
        [ValidateRequest]
        public async Task<ActionResult<GetUserAddressResposneDTO>> GetUserAddress(GetUserAddressRequestDTO request)
        {
            try
            {
                Guid UserId = GetUserId();
                if (UserId == Guid.Empty)
                {
                    return BadRequest("User not authenticated");
                }
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
