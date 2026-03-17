using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs
{
    public record Pagination
    {
        [Required(ErrorMessage = "Page size is required")]
        [Range(1, 100, ErrorMessage = "PageNumber must be greater than 0")]
        public int PageSize { get; set; }

        [Required(ErrorMessage = "Page number is required")]
        [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
        public int PageNumber{ get; set; }
    }
}
