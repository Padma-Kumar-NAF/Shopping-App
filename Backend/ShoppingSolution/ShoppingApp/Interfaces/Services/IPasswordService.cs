namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IPasswordService
    {
        public Task<(byte[], byte[])> HashPasswordAsync(string password);
        public Task<bool> VerifyPasswordAsync(string password, string HashKey, string storedSalt);
    }
}