using ShoppingApp.Models.DTOs.Product;

namespace ShoppingApp.Models.DTOs.Category
{
    public record GetAllCategoryResponseDTO
    {
        public ICollection<CategoryDTO> CategoryList { get; set; } = new List<CategoryDTO>();
    }

    public class CategoryDTO
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductsCount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}