using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShoppingApp.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : BaseController, IAuthenticationController
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthenticationController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<CreateUserResponseDTO>> Register([FromBody] CreateUserRequestDTO requestDTO)
        {

            try
            {
                if (requestDTO == null)
                    return BadRequest("Invalid request");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.CreateUser(requestDTO);

                //if (result == null)
                //    return Conflict("Email already exists");

                return StatusCode(201, result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login( [FromBody] LoginRequestDTO requestDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (requestDTO == null)
                    return BadRequest("Invalid request");

                var result = await _userService.LoginUser(requestDTO);

                if (result == null)
                {
                    return NotFound();
                }

                var token = GenerateToken(
                    result.UserId,
                    result.Name,
                    result.Email,
                    result.Role
                );

                result.Token = token;

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        private string GenerateToken(Guid userId,string userName,string email,string role)
        {
            var keyValue = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyValue))
                throw new InvalidOperationException("JWT Key is not configured.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

            var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var durationValue = _configuration["Jwt:DurationInMinutes"];
            if (!double.TryParse(durationValue, out var expiryMinutes))
                expiryMinutes = 60;

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}