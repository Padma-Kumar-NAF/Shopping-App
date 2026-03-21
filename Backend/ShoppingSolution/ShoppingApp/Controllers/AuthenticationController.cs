using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.User;


namespace ShoppingApp.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private readonly IUserService _userService;

        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [ValidateRequest]
        public async Task<ActionResult<CreateUserResponseDTO>> Register([FromBody] CreateUserRequestDTO requestDTO)
        {
            try
            {
                var result = await _userService.CreateUser(requestDTO);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("login")]
        [ValidateRequest]
        public async Task<ActionResult<LoginResponseDTO>> Login( [FromBody] LoginRequestDTO requestDTO)
        {
            try
            {
                var result = await _userService.LoginUser(requestDTO);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}