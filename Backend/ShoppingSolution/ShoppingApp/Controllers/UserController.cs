using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Controllers 
{
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

        [HttpPost("AddUserDetails")]
        public async Task<ActionResult<AddUserDetailsResponseDTO>> AddUserDetails(AddUserDetailsRequestDTO request)
        {
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

        [HttpPost("GetAllUsers")]
        public async Task<ActionResult<GetUsersResponseDTO>> GetAllUsers(GetUsersRequestDTO request)
        {
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

    }
}
