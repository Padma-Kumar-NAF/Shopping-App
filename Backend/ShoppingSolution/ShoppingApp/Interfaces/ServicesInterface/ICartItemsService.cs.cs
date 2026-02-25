using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICartItemsService
    {
        public Task<IEnumerable<GetCartResponseDTO>> GetCartItems(GetCartRequestDTO request);
        public Task<bool> RemoveAllByCartId(Guid CartId);
    }
}
