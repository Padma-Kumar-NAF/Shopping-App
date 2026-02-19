using ShoppingApp.Interfaces.Repositories;
using ShoppingApp.Models;

namespace ShoppingApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IRepository<Guid, User> _repository;

        public UserRepository(IRepository<Guid, User> repository)
        {
            _repository = repository;
        }

        public async Task<User?> AddUser(User NewUser)
        {
            var user = await _repository.Add(NewUser);
            return user;
        }

    }
}
