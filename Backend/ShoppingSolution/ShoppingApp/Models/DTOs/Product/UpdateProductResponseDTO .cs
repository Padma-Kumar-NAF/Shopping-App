namespace ShoppingApp.Models.DTOs.Product
{
    public record UpdateProductResponseDTO
    {
        public bool IsUpdate { get; set; }
        public Guid CategoryId { get; set; }
    }
}