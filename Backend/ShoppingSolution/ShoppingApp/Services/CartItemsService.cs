using Microsoft.AspNetCore.Cors.Infrastructure;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs;
using ShoppingApp.Repositories;

namespace ShoppingApp.Services
{
    public class CartItemsService : ICartItemsService
    {
        IRepository<Guid, CartItem> _repository;
        ICartItemRepository _cartItemRepository;
        ICartService _cartService;
        public CartItemsService(IRepository<Guid, CartItem> repository, ICartService cartService, ICartItemRepository cartItemRepository)
        {
            _repository = repository;
            _cartService = cartService;
            _cartItemRepository = cartItemRepository;
        }

        public async Task<IEnumerable<GetCartResponseDTO>> GetCartItems(GetCartRequestDTO request)
        {
            var items = await _cartItemRepository.GetCartItemsByUserAsync(
                request.UserId,
                request.PageNumber,
                request.Limit);

            return items;
        }
    }
}
