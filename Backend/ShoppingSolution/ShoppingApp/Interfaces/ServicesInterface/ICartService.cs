using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface ICartService
    {
        public Task<Cart> GetCarts(Guid UserId);
    }
}
