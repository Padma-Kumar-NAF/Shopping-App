namespace ShoppingApp.Models.DTOs.Product
{
    public class UpdateProductResponseDTO
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid StockId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
