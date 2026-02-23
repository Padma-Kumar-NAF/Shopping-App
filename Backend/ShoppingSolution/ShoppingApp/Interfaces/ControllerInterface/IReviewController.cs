using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IReviewController
    {
        public Task<ActionResult<AddReviewResponseDTO>> AddReview(AddReviewRequestDTO request);
        public Task<ActionResult<DeleteReviewResponseDTO>> DeleteReview(DeleteReviewRequestDTO request);
    }
}
