namespace ShoppingApp.Models
{
    public class PromoCode
    {
        public Guid PromoCodeId { get; set; }
        public string PromoCodeName { get; set; } = string.Empty;
        public int DiscountPercentage { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation
        public ICollection<Order>? Orders { get; set; }
    }
}