using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Product
{
    public record GetAllProductsWithFilterRequestDTO
    {
        public Pagination pagination { get; set; } = new Pagination();
        [Required(ErrorMessage ="Low price is required")]
        public int LowPrice { get; set; }
        [Required(ErrorMessage = "High price is required")]
        public int HighPrice { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
