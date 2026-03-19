using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Category
{
    public record EditCategoryRequestDTO
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        public string CategoryName { get; set; } = string.Empty;
    }
}