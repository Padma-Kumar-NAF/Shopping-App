using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Models.DTOs.Category
{
    public record GetAllCategoryResponseDTO
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public ICollection<GetAllProductsResponseDTO> Products { get; set; } = new List<GetAllProductsResponseDTO>();

    }
}
