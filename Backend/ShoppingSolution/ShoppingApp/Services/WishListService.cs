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

        private readonly IUnitOfWork _unitOfWork;

        public WishListService(
            IRepository<Guid, WishList> wishListRepository,
            IRepository<Guid, WishListItems> wishListItemsRepository,
            IRepository<Guid, Product> productRepository,
            IUnitOfWork unitOfWork)
        {
            _wishListRepository = wishListRepository;
            _wishListItemsRepository = wishListItemsRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<AddProductToWishListResponseDTO>> AddToWishListAsync(Guid UserId, Guid ProductId, Guid WishListId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wishList = await _wishListRepository
                .FirstOrDefaultAsync(x => x.WishListId == WishListId && x.UserId == UserId);

                if (wishList == null)
                {
                    throw new AppException("Wishlist not found");
                }

                var product = await _productRepository.GetAsync(ProductId);

                if (product == null)
                    throw new Exception("Product not found");

                var exists = await _wishListItemsRepository.FirstOrDefaultAsync(x => x.WishListId == WishListId && x.ProductId == ProductId);

                if (exists != null)
                    throw new AppException("Product already exists in wishlist");

                var item = new WishListItems
                {
                    WishListId = WishListId,
                    ProductId = ProductId
                };

                await _wishListItemsRepository.AddAsync(item);

                await _unitOfWork.CommitAsync();

                return new ApiResponse<AddProductToWishListResponseDTO>()
                {
                    StatusCode = 200,
                    Data = new AddProductToWishListResponseDTO()
                    {
                        IsSuccess = true
                    },
                    Message = "Product added successfully",
                    Action = "AddProduct"
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Server error occurred while adding product in wishlist", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Server error occurred while adding product in wishlist", ex, 500);
            }
        }

        public async Task<ApiResponse<CreateWishListResponseDTO>> CreateWishListAsync(string WishListName, Guid UserId)
        {
            try
            {
                var isAlreadyExist = await _wishListRepository.GetQueryable().FirstOrDefaultAsync(w => w.WhishListName == WishListName && w.UserId == UserId);

                if (isAlreadyExist != null)
                {
                    throw new AppException("Wishlist already exists");
                }

                var wishList = new WishList
                {
                    UserId = UserId,
                    WhishListName = WishListName
                };

                await _wishListRepository.AddAsync(wishList);
                return new ApiResponse<CreateWishListResponseDTO>()
                {
                    Data = new CreateWishListResponseDTO()
                    {
                        IsCreated = true,
                    },
                    StatusCode = 200,
                    Message = "Wish list created",
                    Action = "AddWishList"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Server error occurred while creating wishlist", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Server error occurred while creating wishlist", ex, 500);
            }

        }

        public async Task<ApiResponse<DeleteWishListResponseDTO>> DeleteWishListAsync(Guid UserId, Guid WishListId)
        {
            try
            {
                var wishList = await _wishListRepository.FirstOrDefaultAsync(x => x.WishListId == WishListId && x.UserId == UserId);

                if (wishList == null)
                {
                    throw new AppException("Wishlist not found");
                }

                await _wishListRepository.DeleteAsync(WishListId);

                return new ApiResponse<DeleteWishListResponseDTO>()
                {
                    Data = new DeleteWishListResponseDTO()
                    {
                        IsDeleted = true,
                    },
                    StatusCode = 200,
                    Message = "Wish list deleted",
                    Action = "DeleteWishList"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Server error occurred while deleting wishlist", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Server error occurred while deleting wishlist", ex, 500);
            }
        }

        public async Task<ApiResponse<GetUserWishListResponseDTO>> GetUserWishListAsync(int limit, int pageNumber, Guid userId)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (limit <= 0) limit = 10;

                var wishlists = await _wishListRepository
                    .GetQueryable()
                    .Where(w => w.UserId == userId)
                    .Include(w => w.WishListItems)!
                    .ThenInclude(i => i.Products)
                    .Skip((pageNumber - 1) * limit)
                    .Take(limit)
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
                            })
                            .ToList()
                    }).ToList()
                };

                return new ApiResponse<GetUserWishListResponseDTO>()
                {
                    Data = result,
                    StatusCode = 200,
                    Message = result.WishList.Any()
                        ? "Wishlist fetched successfully"
                        : "No wishlist found",
                    Action = result.WishList.Any()
                        ? string.Empty
                        : "ShowEmptyPage"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while fetching wishlist", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while fetching wishlist", ex, 500);
            }
        }

        public async Task<ApiResponse<RemoveProductFromWishListResponseDTO>> RemoveFromWishListAsync(Guid UserId, Guid WishListId, Guid ProductId)
        {
            try
            {
                var item = await _wishListItemsRepository.FirstOrDefaultAsync(x => x.WishListId == WishListId && x.ProductId == ProductId);

                if (item == null)
                {
                    throw new AppException("Wishlist item not found");
                }

                await _wishListItemsRepository.DeleteAsync(item.WishListItemsId);

                return new ApiResponse<RemoveProductFromWishListResponseDTO>()
                {
                    Data = new RemoveProductFromWishListResponseDTO()
                    {
                        IsRemoved = true,
                    },
                    Message = "Item removed successfully",
                    Action = "RemoveItem",
                    StatusCode = 200
                };
            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (DbUpdateException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Server error occurred while removing item in wishlist", ex, 500);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Server error occurred while removing item in wishlist", ex, 500);
            }
        }
    }
}