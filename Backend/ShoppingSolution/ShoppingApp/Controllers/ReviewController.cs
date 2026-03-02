using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Interfaces.ControllerInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models.DTOs.Review;
using System.Security.Claims;

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

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Guid.Empty;

            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : Guid.Empty;
        }

        [HttpPost("AddReview")]
        public async Task<ActionResult<AddReviewResponseDTO>> AddReview(AddReviewRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.UserId = GetUserId();
            if(request.UserId == Guid.Empty)
            {
                return BadRequest("User not found");
            }

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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.UserId = GetUserId();
            if (request.UserId == Guid.Empty)
            {
                return BadRequest("User not found");
            }

            try
            {
                var review = await _reviewService.DeleteReview(request.UserId,request.ReviewId);
                return Ok(review);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
