using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.RepositoriesInterface
{
    public interface ICartItemRepository
    {
        public Task<IEnumerable<GetCartResponseDTO>> GetCartItemsByUserAsync(Guid userId,int pageNumber,int pageSize);
    }
}
