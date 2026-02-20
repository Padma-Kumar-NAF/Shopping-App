using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;

namespace ShoppingApp.Repositories
{
    public class UserHashRepository : MasterRepository<Guid, UserHash> , IUserHashRepository
    {
        public UserHashRepository(IRepository<Guid, UserHash> repository) : base(repository)
        {

        }

        public async Task<bool> AddHash(Guid UserId, string SaltValue)
        {
            UserHash userHash = new UserHash();
            userHash.UserId = UserId;
            userHash.SaltValue = SaltValue;
            var result = await _repository.AddAsync(userHash);
            if(result == null)
            {
                return false;
            }
            return true;
        }
    }
}