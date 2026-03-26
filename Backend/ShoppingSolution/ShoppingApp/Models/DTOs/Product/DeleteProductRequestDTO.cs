namespace ShoppingApp.Models.DTOs.Product
{
    public record DeleteProductRequestDTO
    {
        public Guid ProductId { get; set; }
    }
}
