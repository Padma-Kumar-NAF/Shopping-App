using Azure.Core;
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
    [Route("auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase , IAuthenticationController
    {
        private readonly IUserService _userService;

        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<CreateUserResponseDTO>> Register([FromBody] CreateUserRequestDTO requestDTO)
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
                if(result == null)
                {
                    return BadRequest("Invalid Credentials");
                }
                var token = GenerateToken(result.Name , result.Role);
                result.Token = token;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        public string GenerateToken(string UserName , string Role)
        {
            var ClaimList = new List<Claim>
            {
                new Claim(ClaimTypes.Name , UserName),
                new Claim(ClaimTypes.Role , Role),
            };

            var token = new JwtSecurityToken(
                issuer : "",
                audience : "",
                claims : ClaimList,
                expires : DateTime.Now.AddDays(1),
                signingCredentials : new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsMySuperSecureKey123456789PadmaKumar")) , SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
