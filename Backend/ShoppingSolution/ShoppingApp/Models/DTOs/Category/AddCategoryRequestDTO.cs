namespace ShoppingApp.Models.DTOs.Category
{
    public record AddCategoryRequestDTO
    {
        public string CategoryName{ get; set; } = string.Empty;
    }
}
