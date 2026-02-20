using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;

namespace ShoppingApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        protected readonly IRepository<Guid, User> _repository;
        public UserRepository(IRepository<Guid, User> repository)
        {
            _repository = repository;
        }
        public async Task<User?> AddUser(User NewUser)
        {
            var user = await _repository.AddAsync(NewUser);
            return user;
        }

        public async Task<User?> GetUserByMail(string email)
        {
            return await _repository.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
