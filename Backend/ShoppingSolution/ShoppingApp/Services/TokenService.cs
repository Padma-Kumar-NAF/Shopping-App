using Microsoft.IdentityModel.Tokens;
using ShoppingApp.Interfaces.ServicesInterface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShoppingApp.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(Guid userId, string userName, string email, string role)
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

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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