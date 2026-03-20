using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Wishlist;
using System.Security.Claims;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "user")]
    [Route("api/wishlist")]
    [ApiController]
    public class WishListController : BaseController, IWishListController
    {
        private readonly IWishListService _wishlistService;
        public WishListController(IWishListService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [Authorize(Roles = "user")]
        [HttpPost("add-product")]
        [ValidateRequest]
        public async Task<IActionResult> AddToWishListAsync([FromBody] AddProductToWishListRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _wishlistService.AddToWishListAsync(UserId,request.ProductId,request.WishListId);

                return Ok(result);
            }
            catch 
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost("create")]
        [ValidateRequest]
        public async Task<IActionResult> CreateWishList([FromBody] CreateWishListRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _wishlistService.CreateWishListAsync(request.WishListName,UserId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpDelete("delete")]
        [ValidateRequest]
        public async Task<IActionResult> DeleteWishList([FromBody] DeleteWishListRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _wishlistService.DeleteWishListAsync(UserId,request.WishListId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpPost("user-wishlist")]
        [ValidateRequest]
        public async Task<IActionResult> GetUserWishList([FromBody] GetUserWishListRequestDTOClass request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _wishlistService.GetUserWishListAsync(request.pagination.PageSize, request.pagination.PageNumber,UserId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [Authorize(Roles = "user")]
        [HttpDelete("remove-product")]
        [ValidateRequest]
        public async Task<IActionResult> RemoveFromWishList([FromBody] RemoveProductFromWishListRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _wishlistService.RemoveFromWishListAsync(UserId,request.WishListId,request.ProductId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}