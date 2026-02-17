using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Cart
    {
        [Key]
        public Guid CartId {  get; set; }

        [Required]
        public Guid UserId{ get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation 
        public User? User { get; set; } // one to one
        public ICollection<CartItem>? CartItems { get; set; } // one to many
    }
}
