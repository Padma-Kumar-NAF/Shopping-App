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


        /// <summary>
        /// Creates a new address for the authenticated user.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized with the 'user'
        /// role. The request is validated before processing. An exception is thrown if the user is not authorized or if
        /// address creation fails.</remarks>
        /// <param name="request">An object containing the details of the address to be created. Must not be null.</param>
        /// <returns>An IActionResult containing the result of the address creation operation. If successful, the response
        /// includes the details of the newly created address.</returns>
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

        /// <summary>
        /// Deletes a user address from the authenticated user's account.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized with the 'user'
        /// role. The user must provide a valid address identifier in the request. An exception is thrown if the user ID
        /// cannot be retrieved or if the delete operation fails.</remarks>
        /// <param name="request">An object containing the details of the address to be deleted, including the address identifier.</param>
        /// <returns>An IActionResult that indicates the outcome of the delete operation. Returns a success response if the
        /// address is deleted; otherwise, returns an error response.</returns>
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

        /// <summary>
        /// Updates an existing user address with the details provided in the request.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized with the 'user'
        /// role. An exception is thrown if the user ID cannot be determined or if the address update operation
        /// fails.</remarks>
        /// <param name="request">An <see cref="EditUserAddressRequestDTO"/> containing the updated address information. Must include all
        /// required fields for address validation.</param>
        /// <returns>An <see cref="IActionResult"/> containing the updated list of addresses for the authenticated user.</returns>
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

        /// <summary>
        /// Retrieves the list of addresses associated with the authenticated user.
        /// </summary>
        /// <remarks>This action requires the caller to be authenticated and authorized with the 'user'
        /// role. The request is validated before processing. Any exceptions encountered during execution are propagated
        /// to the caller.</remarks>
        /// <param name="request">An object containing the criteria used to filter or retrieve the user's addresses. Cannot be null.</param>
        /// <returns>An IActionResult containing a collection of addresses for the authenticated user. Returns an empty
        /// collection if no addresses are found.</returns>
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