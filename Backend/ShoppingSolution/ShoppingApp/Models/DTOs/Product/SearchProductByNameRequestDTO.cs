using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Product
{
    public record SearchProductByNameRequestDTO
    {
        [Required(ErrorMessage = "Product Name is required")]
        public string ProductName{ get; set; } = string.Empty;
    }
}
