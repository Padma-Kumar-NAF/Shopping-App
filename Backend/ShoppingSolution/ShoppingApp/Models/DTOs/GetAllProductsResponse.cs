using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs
{
    public class GetAllProductsResponse
    {
        public Guid ProductId { get; set; }

        public Guid CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public decimal Price { get; set; }

    }
}
