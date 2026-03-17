using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Review
{
    public record AddReviewResponseDTO
    {
        [Required]
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        public string Summary { get; set; } = string.Empty;
        [Required]
        [Range(1, 5)]
        public int ReviewPoints { get; set; }
    }
}
