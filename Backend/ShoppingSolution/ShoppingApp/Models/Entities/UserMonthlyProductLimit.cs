namespace ShoppingApp.Models.Entities
{
    public class UserMonthlyProductLimit
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int MonthlyLimit { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Product? Product { get; set; }
    }
}