namespace ShoppingApp.Models
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string StripePaymentId { get; set; } = string.Empty;

        // Navigation
        public User? User { get; set; }
        public Order? Order { get; set; }
        public Refund? Refund { get; set; }
    }
}
