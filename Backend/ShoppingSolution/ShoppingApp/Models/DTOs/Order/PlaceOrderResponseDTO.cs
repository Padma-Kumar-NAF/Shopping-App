namespace ShoppingApp.Models.DTOs.Order
{
    public class PlaceOrderResponseDTO
    {
        public bool IsSuccess { get; set; }
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Guid OrderDetailsId { get; set; }
    }
}
