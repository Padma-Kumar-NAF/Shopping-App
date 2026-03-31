using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Cart;
using ShoppingApp.Models.DTOs.Stock;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICartItemsService
    {
        public Task<GetCartResponseDTO> GetCartItems(Guid CartId, Guid UserId, int Limit, int PageNumber);
        public Task<bool> RemoveAllByCartId(Guid CartId);
    }
}
