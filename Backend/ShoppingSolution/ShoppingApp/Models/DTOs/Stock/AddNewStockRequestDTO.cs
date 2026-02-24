namespace ShoppingApp.Models.DTOs.Stock
{
    public class AddNewStockRequestDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
