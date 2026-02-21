namespace ShoppingApp.Models.DTOs
{
    public class SearchProductRequestDTO
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
