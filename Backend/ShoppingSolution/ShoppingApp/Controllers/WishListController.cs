using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Wishlist;
using System.Security.Claims;

namespace ShoppingApp.Controllers
{
    [Route("api/wishlist")]
    [ApiController]
    public class WishListController : BaseController, IWishListController
    {
        private readonly IWishListService _wishlistService;
        public WishListController(IWishListService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpPost("add-product")]
        public async Task<ActionResult<AddProductToWishListResponseDTO>> AddToWishListAsync(AddProductToWishListRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == Guid.Empty)
                return BadRequest("Invalid or missing user token");

            try
            {
                var result = await _wishlistService.AddToWishListAsync(
                    userId,
                    request.ProductId,
                    request.WishListId
                );

                var response = new AddProductToWishListResponseDTO
                {
                    IsSuccess = result
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreateWishListResponseDTO>> CreateWishList(CreateWishListRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == Guid.Empty)
                return BadRequest("Invalid or missing user token");

            try
            {
                var result = await _wishlistService.CreateWishListAsync(
                    request.WishListName,
                    userId
                );

                return Ok(new CreateWishListResponseDTO
                {
                    isCreated = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete")]
        public async Task<ActionResult<DeleteWishListResponseDTO>> DeleteWishList(DeleteWishListRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == Guid.Empty)
                return BadRequest("Invalid or missing user token");

            try
            {
                var result = await _wishlistService.DeleteWishListAsync(
                    userId,
                    request.WishListId
                );

                return Ok(new DeleteWishListResponseDTO
                {
                    IsDeleted = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("user-wishlist")]
        public async Task<ActionResult<GetUserWishListResponseDTO>> GetUserWishList(GetUserWishListRequestDTOClass request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == Guid.Empty)
                return BadRequest("Invalid or missing user token");

            try
            {
                var result = await _wishlistService.GetUserWishListAsync(
                    request.Limit,
                    request.PageNumber,
                    userId
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("remove-product")]
        public async Task<ActionResult<RemoveProductFromWishListResponseDTO>> RemoveFromWishList(RemoveProductFromWishListRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == Guid.Empty)
                return BadRequest("Invalid or missing user token");

            try
            {
                var result = await _wishlistService.RemoveFromWishListAsync(
                    userId,
                    request.WishListId,
                    request.ProductId
                );

                return Ok(new RemoveProductFromWishListResponseDTO
                {
                    Success = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
