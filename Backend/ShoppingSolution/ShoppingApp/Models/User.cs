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
        public string Role { get; set; } = string.Empty; 

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation 
        public UserDetails? UserDetails { get; set; } // one to one
        public Cart? Carts { get; set; } // one to one

        public ICollection<Review>? Reviews { get; set; } // one to many
        public ICollection<Log>? Logs { get; set; } // one to many
        public ICollection<Address>? Addresses { get; set; } // one to many
        public ICollection<Order>? Orders { get; set; } // one to many
    }
}
