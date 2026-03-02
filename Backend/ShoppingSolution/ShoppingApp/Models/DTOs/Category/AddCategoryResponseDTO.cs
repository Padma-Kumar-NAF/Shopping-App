namespace ShoppingApp.Models.DTOs.Category
{
    public class AddCategoryResponseDTO
    {
        public Guid CategoryId { get; set; }
        public string CategoryName {  get; set; } = string.Empty;
    }
}
