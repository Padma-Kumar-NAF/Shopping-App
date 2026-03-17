using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models.DTOs.Stock
{
    public record GetStockResponseDTO
    {
        public Guid StockId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage{ get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
