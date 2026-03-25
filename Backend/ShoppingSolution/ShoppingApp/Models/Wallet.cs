namespace ShoppingApp.Models
{
    public class Wallet
    {
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public int WalletAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        // Navigation
        public User? User { get; set; }
    }
}
