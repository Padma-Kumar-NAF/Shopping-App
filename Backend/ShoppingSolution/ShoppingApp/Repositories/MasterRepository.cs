using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Models;

namespace ShoppingApp.Repositories
{
    public class MasterRepository<K,T> where T : class
    {
        protected readonly IRepository<K, T> _repository;
        public MasterRepository(IRepository<K,T> repository)
        {
            _repository = repository;
        }
    }
}
