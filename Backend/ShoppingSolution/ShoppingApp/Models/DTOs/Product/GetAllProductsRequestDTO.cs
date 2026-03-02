using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Product
{
    public class GetAllProductsRequestDTO
    {
        [Required]
        [Range(1, 100, ErrorMessage = "PageNumber must be greater than 0")]
        public int PageNumber { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
        public int Limit { get; set; }

        public Guid? CategoryId {  get; set; }
    }
}
