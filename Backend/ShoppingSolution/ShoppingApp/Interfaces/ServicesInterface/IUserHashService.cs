namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IUserHashService
    {
        public Task<bool> AddHash(Guid UserId, string SaltValue);
    }
}
