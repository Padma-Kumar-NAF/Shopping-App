namespace ShoppingApp.Interfaces.Repositories
{
    public interface IRepository<K,C> where C : class
    {
        Task<IEnumerable<C>?> GetAll();
        Task<C?> Get(K key);
        Task<C?> Add(C item);
        Task<C?> Delete(K key);
        Task<C?> Update(K key, C entity);
    }
}
