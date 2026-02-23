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

        public UserController(IUserService userService)
        {
            _userService = userService;
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
