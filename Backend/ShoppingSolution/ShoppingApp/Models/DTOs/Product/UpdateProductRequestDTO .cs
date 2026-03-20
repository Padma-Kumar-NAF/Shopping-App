using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Product
{
    public record UpdateProductRequestDTO
    {
        [Required(ErrorMessage = "Product Id is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Category Id is required")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [MinLength(2, ErrorMessage = "Product name must be at least 2 characters")]
        [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Image path is required")]
        public string ImagePath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [MinLength(5, ErrorMessage = "Description must be at least 5 characters")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }
    }
}