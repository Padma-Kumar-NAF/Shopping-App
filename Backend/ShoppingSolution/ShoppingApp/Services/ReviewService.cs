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

        public async Task<DeleteReviewResponseDTO> DeleteReview(Guid ReviewID)
        {
            var review = await _repository.DeleteAsync(ReviewID);
            if(review == null)
            {
                throw new Exception($"Unable to delete {ReviewID}");
            }
            DeleteReviewResponseDTO response = new DeleteReviewResponseDTO();
            response.Summary = review.Summary;
            response.UserId = review.UserId;
            response.ProductId = review.ProductId;
            response.ReviewPoints = review.ReviewPoints;
            return response;
        }
    }
}
