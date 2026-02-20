using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;

namespace ShoppingApp.Repositories
{
    public class UserRepository : MasterRepository<Guid, User> , IUserRepository
    {
        public UserRepository(IRepository<Guid, User> repository) : base(repository)
        {

        }
        public async Task<User?> AddUser(User NewUser)
        {
            var user = await _repository.AddAsync(NewUser);
            return user;
        }
    }
}
