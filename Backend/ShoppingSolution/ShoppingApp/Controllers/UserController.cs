using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;
using System.Security.Claims;

namespace ShoppingApp.Controllers 
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase , IUserController
    {
        private readonly IUserService _userService;
        private readonly IUserDetailsService _userDetailsService;

        public UserController(IUserService userService, IUserDetailsService userDetailsService)
        {
            _userService = userService;
            _userDetailsService = userDetailsService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Guid.Empty;

            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : Guid.Empty;
        }

        //[Authorize(Roles = "User")]
        [HttpPost("AddUserDetails")]
        public async Task<ActionResult<AddUserDetailsResponseDTO>> AddUserDetails(AddUserDetailsRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.UserId = GetUserId();
            if (request.UserId == Guid.Empty)
            {
                return BadRequest("User not authenticated");
            }

            try
            {
                var Id = await _userDetailsService.AddUserDetails(request);
                AddUserDetailsResponseDTO response = new AddUserDetailsResponseDTO();
                response.UserDetailsId = Id;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("GetAllUsers")]
        public async Task<ActionResult<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            request.UserId = GetUserId();
            if (request.UserId == Guid.Empty)
            {
                return BadRequest("User not authenticated");
            }

            try
            {
                var result = await _userService.GetAllUsers(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateUserDetails")]
        public async Task<ActionResult<UpdateProfileResponseDTO>> UpdateUserDetails(UpdateProfileRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            request.UserId = GetUserId();
            if (request.UserId == Guid.Empty)
            {
                return BadRequest("User not authenticated");
            }
            try
            {
                var result = await _userDetailsService.UpdateUserDetails(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
