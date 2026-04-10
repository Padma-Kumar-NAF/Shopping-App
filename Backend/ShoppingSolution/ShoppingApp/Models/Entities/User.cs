using ShoppingApp.Models.Entities;
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

        [Required]
        public bool Active { get; set; }

        // Navigation
        public UserDetails? UserDetails { get; set; }
        public Cart? Cart { get; set; }
        public Wallet? Wallet { get; set; }

        public ICollection<WishList>? WishLists { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Refund>? Refunds { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<Log>? Logs { get; set; }
        public ICollection<Address>? Addresses { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<UserPromoCode>? UserPromoCodes { get; set; }
    }
}
