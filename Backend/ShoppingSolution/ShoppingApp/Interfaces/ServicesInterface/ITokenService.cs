namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ITokenService
    {
        public string GenerateToken(Guid userId, string userName, string email, string role);
    }
}
