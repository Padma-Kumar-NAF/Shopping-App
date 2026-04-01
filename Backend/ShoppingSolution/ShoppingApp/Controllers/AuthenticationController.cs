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

        /// <summary>
        /// Registers a new user with the provided user information.
        /// </summary>
        /// <remarks>This method is accessible via an HTTP POST request to the 'register' endpoint. The
        /// request is validated before processing. If user creation fails due to invalid input or other errors, an
        /// appropriate error response is returned.</remarks>
        /// <param name="requestDTO">The data transfer object containing the details required to create a new user. This parameter must not be
        /// null.</param>
        /// <returns>An ActionResult containing a CreateUserResponseDTO with the details of the newly created user if
        /// registration is successful.</returns>
        [HttpPost("register")]
        [ValidateRequest]
        public async Task<IActionResult> Register([FromBody] CreateUserRequestDTO requestDTO)
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


        /// <summary>
        /// Authenticates a user based on the provided credentials and returns a response containing authentication
        /// details.
        /// </summary>
        /// <remarks>This method is accessed via an HTTP POST request to the 'login' endpoint. It
        /// validates the incoming request and may throw exceptions if authentication fails or if the request is
        /// invalid.</remarks>
        /// <param name="requestDTO">The login request data transfer object containing the user's credentials. This parameter must not be null.</param>
        /// <returns>An <see cref="ActionResult{LoginResponseDTO}"/> containing the user's authentication details if the login is
        /// successful.</returns>
        [HttpPost("login")]
        [ValidateRequest]
        public async Task<IActionResult> Login( [FromBody] LoginRequestDTO requestDTO)
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