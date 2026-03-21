using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IReviewService
    {
        public Task<ApiResponse<AddReviewResponseDTO>> AddReview(Guid userId,AddReviewRequestDTO request);
        public Task<ApiResponse<DeleteReviewResponseDTO>> DeleteReview(Guid UserId , Guid ReviewID);
    }
}
