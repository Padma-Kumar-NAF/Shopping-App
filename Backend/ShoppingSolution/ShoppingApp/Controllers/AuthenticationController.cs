using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase , IAuthenticationControllerInterface
    {
        private readonly IUserService _userService;

        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<CreateUserResponseDTO>> Register(CreateUserRequestDTO requestDTO)
        {
            try
            {
                var result = await _userService.CreateUser(requestDTO);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO requestDTO)
        {
            try
            {
                var result = await _userService.LoginUser(requestDTO);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
