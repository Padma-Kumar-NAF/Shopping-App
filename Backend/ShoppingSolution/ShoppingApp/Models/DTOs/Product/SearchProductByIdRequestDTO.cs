using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Product
{
    public record SearchProductByIdRequestDTO
    {
        [Required(ErrorMessage = "Product name is required")]
        public Guid ProductId { get; set; }
    }
}