using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string SaltValue { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation 
        public UserDetails? UserDetails { get; set; }
        public Cart? Cart { get; set; }

        public ICollection<WishList>? WishLists { get; set; } = new List<WishList>();
        public ICollection<Payment>? Payments { get; set; } = new List<Payment>();
        public ICollection<Refund>? Refunds { get; set; } = new List<Refund>();

        public ICollection<Review>? Reviews { get; set; } = new List<Review>(); 
        public ICollection<Log>? Logs { get; set; } = new List<Log>();
        public ICollection<Address>? Addresses { get; set; } = new List<Address>();  
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
    }
}
