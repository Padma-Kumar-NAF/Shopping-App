using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Review
{
    public record DeleteReviewRequestDTO
    {

        [Required(ErrorMessage = "Review Id is required")]
        public Guid ReviewId { get; set; }
    }
}
