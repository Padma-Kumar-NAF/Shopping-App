using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Wishlist;

namespace ShoppingApp.Services
{
    public class WishListService : IWishListService
    {
        private readonly IRepository<Guid, WishList> _wishListRepository;
        private readonly IRepository<Guid, WishListItems> _wishListItemsRepository;
        private readonly IRepository<Guid, Product> _productRepository;

        public WishListService(
            IRepository<Guid, WishList> wishListRepository,
            IRepository<Guid, WishListItems> wishListItemsRepository,
            IRepository<Guid, Product> productRepository)
        {
            _wishListRepository = wishListRepository;
            _wishListItemsRepository = wishListItemsRepository;
            _productRepository = productRepository;
        }

        public async Task<bool> CreateWishListAsync(string WishListName, Guid UserId)
        {
            var isAlreadyExist = await _wishListRepository
                .GetQueryable()
                .FirstOrDefaultAsync(w => w.WhishListName == WishListName && w.UserId == UserId);

            if (isAlreadyExist != null)
                throw new AppException("Wishlist already exists");

            var wishList = new WishList
            {
                UserId = UserId,
                WhishListName = WishListName
            };

            await _wishListRepository.AddAsync(wishList);
            return true;
        }

        public async Task<bool> AddToWishListAsync(Guid UserId, Guid ProductId, Guid WishListId)
        {
            var wishList = await _wishListRepository
                .FirstOrDefaultAsync(x => x.WishListId == WishListId && x.UserId == UserId);

            if (wishList == null)
                throw new AppException("Wishlist not found");

            var product = await _productRepository.GetAsync(ProductId);

            if (product == null)
                throw new Exception("Product not found");

            var exists = await _wishListItemsRepository
                .FirstOrDefaultAsync(x => x.WishListId == WishListId && x.ProductId == ProductId);

            if (exists != null)
                throw new AppException("Product already exists in wishlist");

            var item = new WishListItems
            {
                WishListId = WishListId,
                ProductId = ProductId
            };

            await _wishListItemsRepository.AddAsync(item);

            return true;
        }

        public async Task<bool> RemoveFromWishListAsync(Guid UserId, Guid WishListId, Guid ProductId)
        {
            var item = await _wishListItemsRepository
                .FirstOrDefaultAsync(x => x.WishListId == WishListId && x.ProductId == ProductId);

            if (item == null)
                throw new AppException("Wishlist item not found");

            await _wishListItemsRepository.DeleteAsync(item.WishListItemsId);

            return true;
        }

        public async Task<bool> DeleteWishListAsync(Guid UserId, Guid WishListId)
        {
            var wishList = await _wishListRepository
                .FirstOrDefaultAsync(x => x.WishListId == WishListId && x.UserId == UserId);

            if (wishList == null)
                throw new AppException("Wishlist not found");

            await _wishListRepository.DeleteAsync(WishListId);

            return true;
        }

        public async Task<GetUserWishListResponseDTO> GetUserWishListAsync(int Limit, int PageNumber, Guid UserId)
        {
            if (PageNumber <= 0) PageNumber = 1;
            if (Limit <= 0) Limit = 10;

            var query = _wishListRepository
            .GetQueryable()
            .Where(w => w.UserId == UserId);

            var hasWishlists = await query.AnyAsync();

            if (!hasWishlists)
                throw new AppException("No wishlist found for this user");

            var wishlists = await query
                .Include(w => w.WishListItems)!
                .ThenInclude(i => i.Products)
                .Skip((PageNumber - 1) * Limit)
                .Take(Limit)
                .ToListAsync();

            var result = new GetUserWishListResponseDTO
            {
                WishList = wishlists.Select(w => new WishListDTO
                {
                    WishListId = w.WishListId,
                    WishListName = w.WhishListName,

                    WishListItems = (w.WishListItems ?? new List<WishListItems>())
                        .Select(i => new WishListItemsDTO
                        {
                            WishListItemsId = i.WishListItemsId,
                            ProductId = i.ProductId,

                            ProductName = i.Products?.Name ?? string.Empty,
                            ProductImage = i.Products?.ImagePath ?? string.Empty

                        }).ToList()

                }).ToList()
            };

            return result;
        }
    }
}