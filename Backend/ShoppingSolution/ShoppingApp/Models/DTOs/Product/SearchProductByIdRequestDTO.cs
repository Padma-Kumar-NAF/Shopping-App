using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Product
{
    public class SearchProductByIdRequestDTO
    {
        [Required(ErrorMessage = "Product name is required")]
        public Guid ProductId { get; set; }
    }
}