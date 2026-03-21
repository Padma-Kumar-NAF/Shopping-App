using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Review
{
    public record DeleteReviewResponseDTO
    {
        public bool IsDeleted { get; set; }
    }
}
