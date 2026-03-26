using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Cart;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "user")]
    [Route("[controller]")]
    [ApiController]
    [ValidateRequest]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }


        /// <summary>
        /// Adds an item to the authenticated user's shopping cart using the specified request data.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized with the 'user'
        /// role. The request is validated before processing. Exceptions that occur during the operation are propagated
        /// to the caller.</remarks>
        /// <param name="request">An object containing the details of the item to add to the cart, including the product identifier and
        /// quantity. Cannot be null.</param>
        /// <returns>An IActionResult that indicates the result of the operation. If successful, the response contains the
        /// updated cart information.</returns>
        [Authorize(Roles = "user")]
        [HttpPost("add-to-cart")]
        [ValidateRequest]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var response = await _cartService.AddCart(UserId,request);
                return Ok(response);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves the authenticated user's shopping cart items using the specified pagination parameters.
        /// </summary>
        /// <remarks>This method requires the caller to be authenticated and authorized with the 'user'
        /// role. The user's identifier is obtained from the current authentication context. The request will be
        /// validated before processing.</remarks>
        /// <param name="request">An object containing pagination information, including the page number and page size to determine which cart
        /// items to return.</param>
        /// <returns>An <see cref="IActionResult"/> containing the user's cart items if the operation succeeds; otherwise, an
        /// error response.</returns>
        [Authorize(Roles = "user")]
        [HttpPost("get-user-cart")]
        [ValidateRequest]
        public async Task<IActionResult> GetCart([FromBody] GetCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.GetUserCarts(UserId, request.Pagination.PageNumber, request.Pagination.PageSize);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// If user Increase or decrease the quantity from cart age page then this controller should be call
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "user")]
        [HttpPost("update-user-cart")]
        [ValidateRequest]
        public async Task<IActionResult> UpdateUserCart([FromBody] UpdateUserCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.UpdateCart(UserId,request.CartId, request.CartItemId, request.ProductId, request.Quantity);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Places an order for all items currently in the authenticated user's cart using the specified address and
        /// payment type.
        /// </summary>
        /// <remarks>This method requires the user to be authenticated and authorized with the 'user'
        /// role. The user must have a valid cart with items to place an order. An exception is thrown if the user ID
        /// cannot be retrieved or if the order placement fails.</remarks>
        /// <param name="request">An object containing the address ID and payment type to be used for the order. This parameter must not be
        /// null.</param>
        /// <returns>An IActionResult that indicates the result of the order placement operation. Returns a success response with
        /// order details if the order is placed successfully; otherwise, returns an error response describing the
        /// failure.</returns>
        [Authorize(Roles = "user")]
        [HttpPost("order-all-from-cart")]
        [ValidateRequest]
        public async Task<IActionResult> PlaceOrderAllFromCarts([FromBody] OrderAllFromCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.PlaceOrderAllFromCart(UserId, request.AddressId, request.PaymentType, request.PromoCode, request.UseWallet, request.StripePaymentId);

                return Ok(result);  
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Removes all items from the authenticated user's shopping cart.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and authorized with the 'user'
        /// role. An exception is thrown if the user ID cannot be determined or if the cart service encounters an error
        /// while removing items.</remarks>
        /// <returns>An IActionResult that indicates the result of the operation. Returns a success response if all items are
        /// removed; otherwise, returns an error response.</returns>
        [Authorize(Roles = "user")]
        [HttpPost("remove-all-from-cart")]
        [ValidateRequest]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.RemoveAllFromCartByUserID(UserId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Removes a specified item from the authenticated user's shopping cart.
        /// </summary>
        /// <remarks>The user must be authenticated and authorized with the 'user' role to access this
        /// endpoint. The method will throw an exception if the user is not authorized or if the specified cart or item
        /// does not exist.</remarks>
        /// <param name="request">An object containing the identifiers for the cart, cart item, and product to be removed.</param>
        /// <returns>An IActionResult that indicates the result of the remove operation. Returns a success response if the item
        /// is removed; otherwise, returns an error response.</returns>
        [Authorize(Roles = "user")]
        [HttpDelete("remove-from-cart")]
        [ValidateRequest]
        public async Task<IActionResult> RemoveFromCart( [FromBody] RemoveFromCartRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();

                var result = await _cartService.RemoveFromCart(UserId, request.CartId, request.CartItemId,request.ProductId);

                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}