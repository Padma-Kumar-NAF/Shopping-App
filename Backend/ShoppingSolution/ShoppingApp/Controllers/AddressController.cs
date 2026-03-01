using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Address;
using System.Security.Claims;

namespace ShoppingApp.Controllers
{
    //[Authorize(Roles = "User")]
    [Route("[controller]")]    
    [ApiController]
    public class AddressController : ControllerBase , IAddressController
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId!);
        }

        [HttpPost("CreateAddress")]
        public async Task<ActionResult<CreateNewAddressResponseDTO>> AddAddress(CreateNewAddressRequestDTO request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            //try
            //{
                request.UserId = GetUserId();
                if(request.UserId == Guid.Empty)
                {
                    return BadRequest("User not found");
                }
                var result = await _addressService.AddAddress(request);
                if (result == null)
                    return StatusCode(500, "Unable to create address at the moment");
            //return Ok(result);
            return CreatedAtAction(nameof(AddAddress), result);
        }

        [HttpPost("GetUserAddress")]
        public async Task<ActionResult<GetUserAddressResposneDTO>> GetUserAddress(GetUserAddressRequestDTO request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                request.UserId = GetUserId();
                if (request.UserId == Guid.Empty)
                {
                    return BadRequest("User not authenticated");
                }
                var addressList = await _addressService.GetUserAddress(request);

                if (addressList == null)
                    return NotFound("No addresses found for this user.");

                return Ok(addressList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteUserAddress")]
        public async Task<ActionResult<bool>> DeleteUserAddress(DeleteUserAddressRequestDTO request)
        {
            if (request == null || request.AddressId == Guid.Empty)
                return BadRequest("Invalid request");

            request.UserId = GetUserId();
            if (request.UserId == Guid.Empty)
                return Unauthorized("User not authenticated");

            var result = await _addressService.DeleteUserAddress(request);

            if (!result)
                return NotFound("Address not found or does not belong to user");

            return Ok(true);
        }
    }
}
