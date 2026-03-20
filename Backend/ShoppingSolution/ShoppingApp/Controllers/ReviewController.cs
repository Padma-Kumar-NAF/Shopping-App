using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Filters;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Controllers
{
    //[Authorize(Roles = "User")]
    [Route("[controller]")]
    [ApiController]

    public class ReviewController : BaseController, IReviewController
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("add-review")]
        [ValidateRequest]
        public async Task<IActionResult> AddReview([FromBody] AddReviewRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var review =await _reviewService.AddReview(request);
                return Ok(review);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("delete-review")]
        [ValidateRequest]
        public async Task<IActionResult> DeleteReview([FromBody] DeleteReviewRequestDTO request)
        {
            try
            {
                var UserId = GetUserIdOrThrow();
                var result = await _reviewService.DeleteReview(UserId,request.ReviewId);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}