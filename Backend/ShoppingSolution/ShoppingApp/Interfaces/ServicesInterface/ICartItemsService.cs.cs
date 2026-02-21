using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICartItemsService
    {
        public Task<IEnumerable<GetCartResponseDTO>> GetCartItems(GetCartRequestDTO request);
    }
}
