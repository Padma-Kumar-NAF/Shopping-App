namespace ShoppingApp.Models.DTOs.Product
{
    public record GetAllProductsWithFilterResponseDTO
    {
        public ICollection<ProductDetails> ProductList { get; set; } = new List<ProductDetails>();
    }
}
