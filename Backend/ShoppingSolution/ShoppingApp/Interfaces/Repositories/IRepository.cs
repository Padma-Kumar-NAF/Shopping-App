namespace ShoppingApp.Interfaces.Repositories
{
    public interface IRepository<D,C> where C : class
    {
        Task<C> GetById(D key);
        Task<C?> Add(D key);
        Task<IEnumerable<C>> GetAll();
        Task<C> Delete(D key);
        Task<C> Update(D key, C entity);
    }
}
