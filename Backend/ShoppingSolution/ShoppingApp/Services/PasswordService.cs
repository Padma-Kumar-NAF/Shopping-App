using Konscious.Security.Cryptography;
using ShoppingApp.Interfaces.ServicesInterface;
using System.Security.Cryptography;
using System.Text;

namespace ShoppingApp.Services
{
    public class PasswordService : IPasswordService
    {
        private const int saltSize = 16;
        private const int degreeOfParallelism = 8;
        private const int iterations = 4;
        private const int memorySize = 1024 * 64;

        public async Task<(byte[], byte[])> HashPasswordAsync(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = degreeOfParallelism,
                MemorySize = memorySize,
                Iterations = iterations
            };

            byte[] hash = await argon2.GetBytesAsync(32);

            return (hash,salt);
        }

        public async Task<bool> VerifyPasswordAsync(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = degreeOfParallelism,
                MemorySize = memorySize,
                Iterations = iterations
            };

            var newHash = await argon2.GetBytesAsync(32);

            return CryptographicOperations.FixedTimeEquals(newHash, hash);
        }
    }
}
