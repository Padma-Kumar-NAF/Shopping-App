using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Category
{
    public record GetAllCategoryRequestDTO
    {
        
        [Required]
        public Pagination Pagination { get; set; } = new Pagination();
    }
}
