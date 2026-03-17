namespace ShoppingApp.Models.DTOs.Stock
{
    public record GetStockRequestDTO
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
    }
}
