using ShoppingApp.Services;
using Xunit;

namespace Testing.Services
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _service = new PasswordService();

        [Fact]
        public async Task HashPasswordAsync_ReturnsNonEmptyHashAndSalt()
        {
            var (hash, salt) = await _service.HashPasswordAsync("mypassword");

            Assert.NotNull(hash);
            Assert.NotNull(salt);
            Assert.Equal(32, hash.Length);   // 32-byte hash
            Assert.Equal(16, salt.Length);   // 16-byte salt
        }

        [Fact]
        public async Task HashPasswordAsync_DifferentCallsProduceDifferentSalts()
        {
            var (_, salt1) = await _service.HashPasswordAsync("password");
            var (_, salt2) = await _service.HashPasswordAsync("password");

            // Salts are random — should differ
            Assert.False(salt1.SequenceEqual(salt2));
        }

        [Fact]
        public async Task VerifyPasswordAsync_CorrectPassword_ReturnsTrue()
        {
            var (hash, salt) = await _service.HashPasswordAsync("correct");
            var storedHash = Convert.ToBase64String(hash);
            var storedSalt = Convert.ToBase64String(salt);

            var result = await _service.VerifyPasswordAsync("correct", storedHash, storedSalt);
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyPasswordAsync_WrongPassword_ReturnsFalse()
        {
            var (hash, salt) = await _service.HashPasswordAsync("correct");
            var storedHash = Convert.ToBase64String(hash);
            var storedSalt = Convert.ToBase64String(salt);

            var result = await _service.VerifyPasswordAsync("wrong", storedHash, storedSalt);
            Assert.False(result);
        }
    }
}
