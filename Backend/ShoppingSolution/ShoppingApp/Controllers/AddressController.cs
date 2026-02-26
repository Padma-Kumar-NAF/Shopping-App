using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Address;

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

        [HttpPost("CreateAddress")]
        public async Task<ActionResult<CreateNewAddressResponseDTO>> AddAddress(CreateNewAddressRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var newAddress = await _addressService.AddAddress(request);
                if (newAddress == null)
                {
                    throw new Exception("Unable to add a Address");
                }
                return Ok(newAddress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GetUserAddress")]
        public async Task<ActionResult<GetUserAddressResposneDTO>> GetUserAddress(GetUserAddressRequestDTO request)
        {
            try
            {
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
    }
}
