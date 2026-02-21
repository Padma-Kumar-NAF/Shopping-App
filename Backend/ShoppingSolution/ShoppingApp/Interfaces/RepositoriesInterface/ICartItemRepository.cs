using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface ICartItemRepository
    {
        public Task<IEnumerable<GetCartResponseDTO>> GetCartItemsByUserAsync(Guid userId,int pageNumber,int pageSize);
    }
}
