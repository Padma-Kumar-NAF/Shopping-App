using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Review
{
    public record AddReviewResponseDTO
    {
        public Guid ReviewId { get; set; }
    }
}
