using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepository<Guid, Review> _repository;
        public ReviewService(IRepository<Guid, Review> repository)
        {
            _repository = repository;
        }

        public async Task<AddReviewResponseDTO> AddReview(AddReviewRequestDTO request)
        {
            var review = new Review
            {
                Summary = request.Summary,
                UserId = request.UserId,
                ProductId = request.ProductId,
                ReviewPoints = request.ReviewPoints
            };

            var added = await _repository.AddAsync(review);
            if(added == null)
            {
                throw new Exception("Can't Able to add a review");
            }

            AddReviewResponseDTO response = new AddReviewResponseDTO();
            response.ReviewId = added.ReviewId;
            response.ProductId = request.ProductId;
            response.ReviewPoints = request.ReviewPoints;
            response.UserId = request.UserId;
            response.Summary = request.Summary;

            return response;
        }

        public async Task<DeleteReviewResponseDTO> DeleteReview(Guid userId, Guid reviewId)
        {
            var review = await _repository.GetAsync(reviewId);

            if (review == null)
                throw new Exception($"Review with Id {reviewId} not found");

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to delete this review");

            await _repository.DeleteAsync(reviewId);

            return new DeleteReviewResponseDTO
            {
                Summary = review.Summary,
                UserId = review.UserId,
                ProductId = review.ProductId,
                ReviewPoints = review.ReviewPoints
            };
        }
    }
}
