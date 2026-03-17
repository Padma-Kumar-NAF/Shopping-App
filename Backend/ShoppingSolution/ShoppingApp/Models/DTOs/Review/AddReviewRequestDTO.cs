using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Review
{
    public record AddReviewRequestDTO
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Product Id is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Review summary is required")]
        [MinLength(5, ErrorMessage = "Review summary must be at least 5 characters")]
        [MaxLength(500, ErrorMessage = "Review summary cannot exceed 500 characters")]
        public string Summary { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Review points must be between 1 and 5")]
        public int ReviewPoints { get; set; }
    }
}
