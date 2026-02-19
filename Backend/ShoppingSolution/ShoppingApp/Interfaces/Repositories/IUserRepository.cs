using ShoppingApp.Models;

namespace ShoppingApp.Interfaces.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> AddUser(User NewUser);
    }
}
