namespace ShoppingApp.Models.DTOs.Cart
{
    public record OrderAllFromCartResponseDTO
    {
        public bool IsSuccess { get; set; }
        public Guid OrderId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal WalletUsed { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
