using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Review
{
    public record DeleteReviewResponseDTO
    {
        [Required]
        public string Summary { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Range(1, 5)]
        public int ReviewPoints { get; set; }
    }
}
