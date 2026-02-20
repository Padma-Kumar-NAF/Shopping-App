using ShoppingApp.Models;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface IUserRepository
    {
        public Task<User?> AddUser(User NewUser);
        public Task<User?> GetUserByMail(string Email);
    }
}
