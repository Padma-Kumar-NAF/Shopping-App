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
                var Result =await _reviewService.AddReview(UserId,request);
                return Ok(Result);
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