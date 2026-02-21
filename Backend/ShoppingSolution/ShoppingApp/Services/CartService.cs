using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;

namespace ShoppingApp.Services
{
    public class CartService : ICartService
    {
        IRepository<Guid, Cart> _repository;
        public CartService(IRepository<Guid, Cart> repository)
        {
            _repository = repository;
        }

        public async Task<Cart> GetCarts(Guid UserId)
        {
            var Cart = await _repository.GetAsync(UserId);
            if(Cart == null)
            {
                throw new NotFoundException("No Cart Found");
            }
            return Cart;
        }
    }
}
