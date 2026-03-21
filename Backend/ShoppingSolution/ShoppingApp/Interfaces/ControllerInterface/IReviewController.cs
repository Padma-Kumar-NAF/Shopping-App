using Microsoft.AspNetCore.Mvc;
using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Interfaces.ControllerInterface
{
    public interface IReviewController
    {
        public Task<IActionResult> AddReview(AddReviewRequestDTO request);
        public Task<IActionResult> DeleteReview(DeleteReviewRequestDTO request);
    }
}
