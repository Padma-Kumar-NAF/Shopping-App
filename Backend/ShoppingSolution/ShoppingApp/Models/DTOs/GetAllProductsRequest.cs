namespace ShoppingApp.Models.DTOs
{
    public class GetAllProductsRequest
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public Guid CategoryId {  get; set; }
    }
}
