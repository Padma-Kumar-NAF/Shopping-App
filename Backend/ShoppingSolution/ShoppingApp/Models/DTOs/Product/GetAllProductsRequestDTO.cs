namespace ShoppingApp.Models.DTOs.Product
{
    public class GetAllProductsRequestDTO
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public Guid? CategoryId {  get; set; }
    }
}
