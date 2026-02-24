namespace ShoppingApp.Models.DTOs.Order
{
    public class PlaceOrderRequestDTO
    {
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }
        public int TotalProductsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PlaceOrderItemDTO> Items { get; set; } = new();
    }

    public class PlaceOrderItemDTO
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
