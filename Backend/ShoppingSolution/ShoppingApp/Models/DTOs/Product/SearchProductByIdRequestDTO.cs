using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Product
{
    public record SearchProductByIdRequestDTO
    {
        [Required(ErrorMessage = "Product Id is required")]
        public Guid ProductId { get; set; }
    }
}