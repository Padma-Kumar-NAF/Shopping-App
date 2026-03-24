namespace ShoppingApp.Models.DTOs.Product
{
    public class GetAllProductsWithFilterResponseDTO
    {
        public ICollection<ProductDetails> ProductList { get; set; } = new List<ProductDetails>();
    }
}
