namespace ShoppingApp.Models.DTOs.Product
{
    public record GetAllProductsRequestDTO
    {
        public Pagination pagination { get; set; } = new Pagination();
    }
}
