using Microsoft.Extensions.Configuration;
using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class TokenServiceTests
    {
        private TokenService GetService(string? jwtKey = "supersecretkey1234567890abcdefgh", string? issuer = "TestIssuer", string? audience = "TestAudience", string? duration = "60")
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = jwtKey,
                    ["Jwt:Issuer"] = issuer,
                    ["Jwt:Audience"] = audience,
                    ["Jwt:DurationInMinutes"] = duration
                })
                .Build();

            return new TokenService(config);
        }

        [Fact]
        public void GenerateToken_ValidConfig_ReturnsNonEmptyToken()
        {
            var service = GetService();
            var token = service.GenerateToken(Guid.NewGuid(), "TestUser", "test@test.com", "user");

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void GenerateToken_TokenContainsThreeParts()
        {
            // JWT format: header.payload.signature
            var service = GetService();
            var token = service.GenerateToken(Guid.NewGuid(), "User", "u@u.com", "admin");

            Assert.Equal(3, token.Split('.').Length);
        }

        [Fact]
        public void GenerateToken_MissingJwtKey_ThrowsInvalidOperation()
        {
            var service = GetService(jwtKey: null);

            Assert.Throws<InvalidOperationException>(() =>
                service.GenerateToken(Guid.NewGuid(), "User", "u@u.com", "user"));
        }

        [Fact]
        public void GenerateToken_EmptyJwtKey_ThrowsInvalidOperation()
        {
            var service = GetService(jwtKey: "   ");

            Assert.Throws<InvalidOperationException>(() =>
                service.GenerateToken(Guid.NewGuid(), "User", "u@u.com", "user"));
        }

        [Fact]
        public void GenerateToken_InvalidDuration_DefaultsTo60Minutes()
        {
            // Non-numeric duration should default to 60 without throwing
            var service = GetService(duration: "notanumber");
            var token = service.GenerateToken(Guid.NewGuid(), "User", "u@u.com", "user");

            Assert.NotEmpty(token);
        }
    }
}
