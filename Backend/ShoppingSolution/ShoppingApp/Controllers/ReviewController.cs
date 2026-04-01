using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Controllers
{
    [Authorize(Roles = "user")]
    [Route("[controller]")]
    [ApiController]
    [ValidateRequest]
    public class ReviewController : BaseController, IReviewController
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Adds a new review for a product or service based on the details provided in the request.
        /// </summary>
        /// <remarks>The user must be authenticated to call this method. An exception is thrown if the
        /// user ID cannot be determined or if the review cannot be added.</remarks>
        /// <param name="request">An object containing the details of the review to add, including the review content and any associated
        /// metadata. Cannot be null.</param>
        /// <returns>An IActionResult that represents the result of the operation. On success, the response includes the details
        /// of the added review.</returns>
        [HttpPost("add-review")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result =await _reviewService.AddReview(UserId,request);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes a review identified by the specified review ID.
        /// </summary>
        /// <remarks>This method requires the user to be authenticated. If the review ID does not exist,
        /// an appropriate error will be thrown.</remarks>
        /// <param name="request">The request object containing the review ID to be deleted.</param>
        /// <returns>Returns a result indicating the success or failure of the deletion operation.</returns>
        [HttpPost("delete-review")]
        public async Task<IActionResult> DeleteReview([FromBody] DeleteReviewRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var Result = await _reviewService.DeleteReview(UserId,request.ReviewId);
                return Ok(Result);
            }
            catch
            {
                throw;
            }
        }
    }
}