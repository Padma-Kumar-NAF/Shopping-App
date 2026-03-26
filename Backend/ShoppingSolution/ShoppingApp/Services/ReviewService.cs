using Microsoft.EntityFrameworkCore;
using ShoppingApp.Exceptions;
using ShoppingApp.Interfaces.RepositoriesInterface;
using ShoppingApp.Interfaces.ServicesInterface;
using ShoppingApp.Models;
using ShoppingApp.Models.DTOs.Review;

namespace ShoppingApp.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepository<Guid, Review> _repository;
        private readonly IRepository<Guid, User> _userRepository;

        public ReviewService(IRepository<Guid, Review> repository, IRepository<Guid, User> userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<AddReviewResponseDTO>> AddReview(Guid userId,AddReviewRequestDTO request)
        {
            try
            {
                var user = await _userRepository.GetAsync(userId);
                if (user == null)
                {
                    throw new AppException("User not found", 404);
                }

                var existingReview = await _repository.GetQueryable().FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == request.ProductId);

                if (existingReview != null)
                {
                    throw new AppException("User has already submitted a review for this product", 409);
                }

                var review = new Review
                {
                    Summary = request.Summary,
                    UserId = userId,
                    ProductId = request.ProductId,
                    ReviewPoints = request.ReviewPoints
                };

                var added = await _repository.AddAsync(review);

                return new ApiResponse<AddReviewResponseDTO>()
                {
                    Data = new AddReviewResponseDTO
                    {
                        ReviewId = added!.ReviewId
                    },
                    StatusCode = 200,
                    Action = "AddReview",
                    Message = "Review added successfully"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while Searching product", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while Searching product", ex, 500);
            }
        }

        public async Task<ApiResponse<DeleteReviewResponseDTO>> DeleteReview(Guid userId, Guid reviewId)
        {
            try
            {
                var review = await _repository.GetAsync(reviewId);

                if (review == null)
                    throw new Exception($"Review not found");

                if (review.UserId != userId)
                    throw new UnauthorizedAccessException("You are not allowed to delete this review");

                await _repository.DeleteAsync(reviewId);

                return new ApiResponse<DeleteReviewResponseDTO>()
                {
                    Data = new DeleteReviewResponseDTO
                    {
                        IsDeleted = true,
                    },
                    StatusCode = 200,
                    Message = "Review deleted successfully",
                    Action = "DeleteReview"
                };
            }
            catch (AppException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new AppException("Error while deleting review", ex, 500);
            }
            catch (Exception ex)
            {
                throw new AppException("Something went wrong while deleting review", ex, 500);
            }
        }
    }
}