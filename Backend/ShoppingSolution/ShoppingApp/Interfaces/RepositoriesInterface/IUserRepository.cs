using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.User;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface IUserRepository
    {
        public Task<User?> AddUser(User NewUser);
        public Task<User?> GetUserByMail(string Email);
        public Task<IEnumerable<GetUsersResponseDTO>> GetUsers(int Limit, int PageNumber);
    }
}
