namespace ShoppingApp.Models.DTOs.Order
{
    public class UpdateOrderRequestDTO
    {
        public Guid OrderId {  get; set; }
        public string OrderStatus { get; set; } = string.Empty;
    }
}
