using ShoppingApp.Contexts;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;

namespace ShoppingApp.Repositories
{
    public class UserRepository : Repository<Guid, User>, IUserRepository
    {
        //protected readonly IRepository<Guid, User> _repository;
        //public UserRepository(IRepository<Guid, User> repository)
        //{
        //    _repository = repository;
        //}

        public UserRepository(ShoppingContext context) : base(context)
        {

        }
        public async Task<User?> AddUser(User NewUser)
        {
            var user = await base.AddAsync(NewUser);
            return user;
        }

        public async Task<User?> GetUserByMail(string email)
        {
            return await base.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
