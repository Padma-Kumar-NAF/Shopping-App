using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Interfaces.ServicesInterface
{
    public interface IReviewService
    {
        public Task<AddReviewResponseDTO> AddReview(AddReviewRequestDTO request);
        public Task<DeleteReviewResponseDTO> DeleteReview(Guid UserId , Guid ReviewID);
    }
}
