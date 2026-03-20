using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Category
{
    public record GetProductsByCategoryRequestDTO
    {
        [Required(ErrorMessage = "Category Id is required")]
        public Guid CategoryId { get; set; }
    }
}