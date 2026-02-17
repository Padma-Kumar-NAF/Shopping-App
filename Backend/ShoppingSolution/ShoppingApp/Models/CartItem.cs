using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class CartItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CartId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        //[Required]
        //public Guid UserId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation 
        public Cart? Cart { get; set; } // many to one
        public Product? Product { get; set; } // many to one
    }
}
