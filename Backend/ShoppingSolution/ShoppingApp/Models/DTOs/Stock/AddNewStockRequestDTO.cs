namespace ShoppingApp.Models.DTOs.Stock
{
    public record AddNewStockRequestDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
