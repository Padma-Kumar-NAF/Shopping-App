using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface IUserHashRepository
    {
        public Task<bool> AddHash(Guid UserId , string SaltValue);
    }
}