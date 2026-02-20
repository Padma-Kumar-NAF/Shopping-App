using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;

namespace ShoppingApp.Services
{
    public class UserHashService : IUserHashService
    {
        private readonly IUserHashRepository _userHashRepository;
        public UserHashService(IUserHashRepository userHashRepository)
        {
            _userHashRepository = userHashRepository;
        }
        public async Task<bool> AddHash(Guid UserId, string SaltValue)
        {
            return await _userHashRepository.AddHash(UserId, SaltValue);
        }
    }
}
