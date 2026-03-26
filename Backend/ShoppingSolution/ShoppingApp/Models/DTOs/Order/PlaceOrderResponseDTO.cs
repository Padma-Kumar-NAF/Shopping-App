namespace ShoppingApp.Models.DTOs.Order
{
    public class PlaceOrderResponseDTO
    {
        public bool IsSuccess { get; set; }
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Guid OrderDetailsId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal WalletUsed { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
