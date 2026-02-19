using ShoppingApp.Interfaces.Service;

namespace ShoppingApp.Services
{
    public class PasswordService : IPasswordService
    {
        public byte[] HashPassword(string password, byte[]? dbHashKey, out byte[]? hashkey)
        {
            throw new NotImplementedException();
        }
    }
}
