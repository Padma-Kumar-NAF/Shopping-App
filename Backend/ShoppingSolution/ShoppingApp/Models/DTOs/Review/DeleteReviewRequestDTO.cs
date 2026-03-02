using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Review
{
    public class DeleteReviewRequestDTO
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Review Id is required")]
        public Guid ReviewId { get; set; }
    }
}
