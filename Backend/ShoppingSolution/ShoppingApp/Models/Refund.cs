namespace ShoppingApp.Models
{
    public class Refund
    {
        public Guid RefundId { get; set; }
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public int RefundAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation 
        public User? User { get; set; }
        public Payment? Payment { get; set; }
    }
}
