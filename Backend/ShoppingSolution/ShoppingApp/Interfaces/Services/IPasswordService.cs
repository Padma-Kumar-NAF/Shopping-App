namespace ShoppingApp.Interfaces.Service
{
    public interface IPasswordService
    {
        public byte[] HashPassword(string password, byte[]? dbHashKey, out byte[]? hashkey);
    }
}