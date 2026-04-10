namespace ShoppingApp.Models.Entities
{
    public class UserPromoCode
    {
        public Guid UserPromoCodeId { get; set; }
        public Guid UserId { get; set; }
        public string PromoCodeName { get; set; } = string.Empty;
        public int DiscountPercentage { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation
        public User? User { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
