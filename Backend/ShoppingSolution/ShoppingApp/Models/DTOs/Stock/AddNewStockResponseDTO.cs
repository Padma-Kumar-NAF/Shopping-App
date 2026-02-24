namespace ShoppingApp.Models.DTOs.Stock
{
    public class AddNewStockResponseDTO
    {
        public Guid StockId {  get; set; }
        public Guid ProductId {  get; set; }
        public int Quantity { get; set; }
    }
}
