using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Controllers
{
    //[Authorize(Roles = "User")]
    [Route("[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase , IReviewController
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("AddReview")]
        public async Task<ActionResult<AddReviewResponseDTO>> AddReview(AddReviewRequestDTO request)
        {
            try
            {
                var review =await _reviewService.AddReview(request);
                return Ok(review);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteReview")]
        public async Task<ActionResult<DeleteReviewResponseDTO>> DeleteReview(DeleteReviewRequestDTO request)
        {
            try
            {
                var review = await _reviewService.DeleteReview(request.ReviewId);
                return review;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
